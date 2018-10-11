using System;

namespace ETModel
{
	public static class TimeHelper
	{
		private static readonly long epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc).Ticks;
		/// <summary>
		/// 客户端时间
		/// </summary>
		/// <returns></returns>
		public static long ClientNow()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000;
		}

		public static long ClientNowSeconds()
		{
			return (DateTime.UtcNow.Ticks - epoch) / 10000000;
		}
        /// <summary>
        /// 获取当前本地时间戳
        /// </summary>
        /// <returns></returns>      
        public static long GetCurrentTimeUnix()
        {
            TimeSpan cha = (DateTime.Now - TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1)));
            long t = (long)cha.TotalSeconds;
            return t;
        }
        /// <summary>
        /// 时间戳转换为本地时间对象
        /// </summary>
        /// <returns></returns>      
        public static DateTime GetUnixDateTime(long unix)
        {
            //long unix = 1500863191;
            DateTime dtStart = TimeZone.CurrentTimeZone.ToLocalTime(new DateTime(1970, 1, 1));
            DateTime newTime = dtStart.AddSeconds(unix);
            return newTime;
        }

        /// <summary>
        /// 登陆前是客户端时间,登陆后是同步过的服务器时间
        /// </summary>
        /// <returns></returns>
        public static long Now()
		{
			return ClientNow();
		}
    }
}