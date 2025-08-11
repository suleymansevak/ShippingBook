using System;
namespace ShippingBook.Core.Entities
{
    public class BaseResponse<T>
    {
        public string Message { get; set; }
        public T Data { get; set; }

        public BaseResponse(string message, T model)
        {

            this.Message = message;
            this.Data = model;

        }
    }
}

