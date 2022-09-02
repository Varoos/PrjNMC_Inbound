using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;

namespace PrjNMC_Inbound
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IFocusService
    {
        [OperationContract]
        [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        BodyStyle = WebMessageBodyStyle.Wrapped,
        UriTemplate = "Getlogin")]     
        ClsProperties.LogingResult Getlogin(string User_Name, string Password, string Company_Code);

        [OperationContract]
        [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "SalesInvoice")]
        ClsSalesInvoice.SalesResponse SalesInvoice(ClsSalesInvoice.SalesInvoice objfocus);

        [OperationContract]
        [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "SalesReturn")]
        ClsSalesInvoice.SalesResponse SalesReturn(ClsSalesInvoice.SalesInvoice objfocus);

        [OperationContract]
        [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "Purchase")]
        ClsSalesInvoice.SalesResponse Purchase(ClsSalesInvoice.SalesInvoice objfocus);

        [OperationContract]
        [WebInvoke(Method = "POST",
        ResponseFormat = WebMessageFormat.Json,
        UriTemplate = "PurchaseReturn")]
        ClsSalesInvoice.SalesResponse PurchaseReturn(ClsSalesInvoice.SalesInvoice objfocus);
    }
}
