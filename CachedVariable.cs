using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Utilities
{
    /// <summary>
    /// a cached variable which is able to invalidate
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class CachedVariable<T> : Interfaces.ICanInvalidate<T>
    {
        volatile bool Updated;
        bool mLocked = false;
        Locker mLocker;
        T mValue;
        object mSyncTarget;
        MemberInfo mMemberInfo;

        /// <summary>
        /// is property or not
        /// </summary>
        bool mIsProperty;

        /// <summary>
        /// is a setter
        /// </summary>
        bool mIsSetter = false;
        public T Value
        {
            get
            {
                return mLocker.Synchronized<T>(() => mValue);
            }
            set
            {
                mLocker.Synchronized(() => {
                    Updated = true;
                    mValue = value;
                });
            }
        }

        public virtual bool Invalidate()
        {
            if (!Updated) return false;
            if (mMemberInfo == null) return false;
            Updated = false;
            T val = this.Value;
            if (!mIsSetter)
            {
                if (mIsProperty)
                {
                    PropertyInfo prop = mMemberInfo as PropertyInfo;
                    prop.SetValue(mSyncTarget, val, new object[] { 0 });
                }
                else
                {
                    FieldInfo field = mMemberInfo as FieldInfo;
                    field.SetValue(mSyncTarget, val);
                }
            }
            else
            {
                MethodInfo method = (MethodInfo)mMemberInfo;
                method.Invoke(mSyncTarget, new object[] { val });
            }
            return true;
        }

        public static implicit operator T(CachedVariable<T> t)
        {
            return t.Value;
        }
        public CachedVariable(object syncTarget, String fieldName, bool IsLocked = false)
        {
            mLocker = new Locker(IsLocked);
            Type TargetType = syncTarget.GetType();
            this.mSyncTarget = syncTarget;

            MemberInfo[] infos = null;
            infos = TargetType.GetMember("set_" + fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | BindingFlags.Instance);
            if (infos.Length > 0)
            {
                mMemberInfo = infos[0];
                mIsSetter = true;
            }
            else
            {
                mMemberInfo = TargetType.GetMethod("set_" + fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | BindingFlags.Instance);
                if (mMemberInfo != null)
                {
                    mIsSetter = true;
                }
                else
                {
                    TargetType.GetMember(fieldName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public | BindingFlags.Instance);
                    if (infos.Length > 0)
                    {
                        mMemberInfo = infos[0];
                        if (mMemberInfo is System.Reflection.PropertyInfo)
                        {
                            mIsProperty = true;
                        }
                        else if (mMemberInfo is System.Reflection.FieldInfo)
                        {
                            mIsProperty = false;
                        }
                    }
                }
            }
        }
    }
}
