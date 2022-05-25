using System;
using System.Collections.Generic;
using System.Text;

namespace WayaPay.Net.SDK
{
    public static class BaseConstants
    {
        public const string WayPayBaseEndPointStaging = "https://services.staging.wayapay.ng/payment-gateway/";
        public const string WayPayBaseEndPointProduction = "https://services.staging.wayapay.ng/payment-gateway/";

        public const string WapPayFrontEndStaging = "https://pay.staging.wayapay.ng/?_tranId={TranID}";
        public const string WapPayFrontEndProduction = "https://pay.staging.wayapay.ng/?_tranId={TranID}";

        //api/v1/request/transaction
        public const string WayPayInitializeTransactionEndPoint = "api/v1/request/transaction";
        public const string WayPayTransactionVerificationEndPoint = "api/v1/reference/query";

        /// <summary>
        /// Application - Json Content Type
        /// </summary>
        public const string ContentTypeHeaderJson = "application/json";


        /// <summary>
        /// Authorization HTTP Header
        /// </summary>
        public const string AuthorizationHeaderType = "Bearer";
    }
}
