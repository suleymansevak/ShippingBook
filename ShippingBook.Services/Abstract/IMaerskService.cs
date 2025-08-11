using System;
using ShippingBook.Core.Entities;

namespace ShippingBook.Services.Abstract
{
    public interface IMaerskService
    {
        Task<BaseResponse<MaerskResponse>> GetTable(MaerskRequest request);
    }
}

