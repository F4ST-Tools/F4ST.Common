using System.Collections.Generic;

namespace F4ST.Common.Models
{
    public class BaseResponse
    {
        /// <summary>
        /// کد وضعیت
        /// </summary>
        public int Status { get; set; }

        /// <summary>
        /// متن پیام کد وضعیت
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// لیست خطاهای بازگشتی در صورت نیاز
        /// </summary>
        public Dictionary<string, string> ValidationResult { get; set; }

    }

    public class BaseResponse<T> : BaseResponse
        where T : class
    {
        public T Item { get; set; }
        public IEnumerable<T> Items { get; set; }
    }

}