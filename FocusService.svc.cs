using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.Web.Configuration;
using System.IdentityModel.Protocols.WSTrust;
using System.Security.Claims;
using System.Security.Principal;
using System.Data;
using System.ServiceModel.Channels;
using System.Collections;
using static PrjNMC_Inbound.ClsProperties;

namespace PrjNMC_Inbound
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the class name "Service1" in code, svc and config file together.
    // NOTE: In order to launch WCF Test Client for testing this service, please select Service1.svc or Service1.svc.cs at the Solution Explorer and start debugging.
    public class FocusService : IFocusService
    {
        int ccode = 0;
        Clsdata cls = new Clsdata();
        string strerror = "";
        string sessionId = "";
        string serverip = WebConfigurationManager.AppSettings["Server_Ip"];
        string username = WebConfigurationManager.AppSettings["User_Name"];
        string Password = WebConfigurationManager.AppSettings["Password"];
        string companycode = WebConfigurationManager.AppSettings["companyCode"];
        int writelog = Convert.ToInt32(WebConfigurationManager.AppSettings["WriteLog"]);
        string AccessToken = "";
        string doctype = "";

        #region GetLogin               
        public ClsProperties.LogingResult Getlogin(string User_Name, string Password, string Company_Code)
        {
            ClsProperties.LogingResult obj = new ClsProperties.LogingResult();
            try
            {

                string companyid = Company_Code;
                ccode = cls.GetCompanyId(companyid);
                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Getlogin", writelog);
                if (companyid == "")
                {
                    obj.iStatus = 0; obj.sMessage = "Company code should not be blank"; obj.Auth_Token = "";
                }
                if (User_Name == "" || Password == "")
                {
                    obj.iStatus = 0; obj.sMessage = "User_Name or Passwrod should not be blank"; obj.Auth_Token = "";
                }

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + companyid, writelog);

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + User_Name, writelog);
                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + Password, writelog);

                string sSessionId = "";

                ClsProperties.Datum datanum = new ClsProperties.Datum();
                datanum.CompanyCode = Company_Code;
                datanum.Username = User_Name;
                datanum.password = Password;
                List<ClsProperties.Datum> lstd = new List<ClsProperties.Datum>();
                lstd.Add(datanum);
                ClsProperties.Lolgin lngdata = new ClsProperties.Lolgin();
                lngdata.data = lstd;
                string sContent = JsonConvert.SerializeObject(lngdata);
                using (WebClient client = new WebClient())
                {
                    client.Headers.Add("Content-Type", "application/json");
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    client.Timeout = 10 * 60 * 1000;
                    var arrResponse = client.UploadString("http://" + serverip + "/focus8API/Login", sContent);
                    //returnObject = new clsDeserialize().Deserialize<RootObject>(arrResponse);
                    ClsProperties.Resultlogin lng = JsonConvert.DeserializeObject<ClsProperties.Resultlogin>(arrResponse);


                    sSessionId = lng.data[0].fSessionId;
                    if (lng.data[0].fSessionId == null || lng.data[0].fSessionId == "" || lng.data[0].fSessionId == "-1")
                    {
                        obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                        return obj;

                    }
                    else

                    {
                        // bool flg = logout(sSessionId, serverip);
                        client.Headers.Add("fSessionId", sSessionId);
                        //client.Timeout = 10 * 60 * 1000;
                        arrResponse = client.DownloadString("http://" + serverip + "/focus8API/Logout");
                    }
                }

                int iloginhandle = 1;
                if (iloginhandle <= 0)
                {
                    obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                }
                else
                {
                    Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Token is generating", writelog);
                    //int iloginhandle = 1;
                    AuthenticationModule objAuth = new AuthenticationModule();
                    string authtoken = objAuth.GenerateTokenForUser(companyid, iloginhandle);
                    Clsdata.LogFile("7HRVSTLogin", DateTime.Now + authtoken, writelog);
                    if (authtoken != "")
                    {
                        obj.iStatus = 1; obj.sMessage = "Token Generated"; obj.Auth_Token = authtoken;

                        Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Token is Generated", writelog);
                    }
                    else
                    {
                        obj.iStatus = 0; obj.sMessage = "User_Name or Password is mismatch"; obj.Auth_Token = "";
                    }
                }
            }
            catch (Exception ex)
            {
                obj.iStatus = 0; obj.sMessage = ex.Message; obj.Auth_Token = "";

                Clsdata.LogFile("7HRVSTLogin", DateTime.Now + "Getlogin excetpion:" + ex.Message, writelog);
            }
            return obj;
        }
        #endregion

        #region Invoice
        public ClsSalesInvoice.SalesResponse SalesInvoice(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "SalesInvoice";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create Invoice", writelog);
                objreturn = CreateInvoice(objfocus, ccode);
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreateInvoice(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateInvoice", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "Entered SalesResponse", writelog);
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(0, 2)));
                int checkindate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(0, 2)));
                int bookingdate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + checkindate.ToString(), writelog);
                if (objfocus.Header.CustomerName == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.CustomerName, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.CustomerName;
                    return objreturn;
                }
                else
                {
                    if (!IsNameExist(ccode, "mcore_Account", objfocus.Header.CustomerName, " and iAccountType = 5"))
                    {
                        int a = CreateMaster(objfocus.Header.CustomerName, ccode, "Account");
                        if (a == -1)
                        {
                            Clsdata.LogFile(doctype, "Posting of New Customer " + objfocus.Header.CustomerName + " Failed", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Posting of New Customer " + objfocus.Header.CustomerName + " Failed";
                            return objreturn;
                        }
                        else
                        {
                            //objfocus.Header.CustomerName = a.ToString();
                        }
                    }
                }
                //Clsdata.LogFile(doctype, "objfocus.Header.Customer = " + objfocus.Header.CustomerName.ToString(), writelog);
                //string strDueDt = getDueDate(objfocus.Body[0].CheckOutDate, objfocus.Header.CustomerName.ToString());
                //dt = new DateTime(Convert.ToInt32(strDueDt.Substring(6, 4)), Convert.ToInt32(strDueDt.Substring(3, 2)), Convert.ToInt32(strDueDt.Substring(0, 2)));
                //Clsdata.LogFile(doctype, "due date" + dt.ToString(), writelog);
                //int duedate = cls.GetDateToInt(dt);
                //Clsdata.LogFile(doctype, "int due date" + duedate.ToString(), writelog);
                string curQry = $@"select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}'";
                DataSet cds = Clsdata.GetData(curQry, ccode);
                Clsdata.LogFile(doctype, curQry, writelog);
                if (cds.Tables[0].Rows.Count > 0)
                {
                    objfocus.Header.CurrencyName = cds.Tables[0].Rows[0]["iCurrencyId"].ToString();
                }

                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }
                #region HardcodedArea
                string FaTag_id = "CBTD";//fixed as Cosmobeds Travel DMCC
                string PlaceofSupply = objfocus.Header.CustomerCountry.ToUpper() == "AE" ? "Dubai" : "Export";
                string Juridction = "Dubai";
                #endregion HardcodedArea

                Hashtable header = new Hashtable
                {
                    { "Date",docdate  },
                    { "CustomerAC__Name",objfocus.Header.CustomerName},
                    { "DueDate",docdate },
                    { "Company Master__Code",FaTag_id },
                    { "Place of supply__Name",PlaceofSupply },
                    { "Jurisdiction__Name",Juridction },
                    { "Transaction Type__Name","Web Booking" },
                    { "Salesman__Name", "Web" },
                    { "ExchangeRate", objfocus.Header.ExchangeRate },
                    { "Hotel Master__Name", objfocus.Header.HotelMaster },
                    { "ConfirmationNumber", objfocus.Header.ConfirmationNo },
                    { "Currency__Id",objfocus.Header.CurrencyName },
                    { "HotelCity", objfocus.Header.HotelCity },
                    { "HotelCountry", objfocus.Header.HotelCountry },
                    { "HotelPhoneNumber", objfocus.Header.HotelPhoneNumber },
                    { "SupplierConfirmationNumber", objfocus.Header.SupplierConfirmationNumber },
                    { "ClientConfirmationNumber", objfocus.Header.ClientConfirmationNumber },
                    { "AgencyVATNumber", objfocus.Header.AgencyVATNumber },
                    { "AgencyAddress", objfocus.Header.AgencyAddress },
                    { "Status__Name", objfocus.Header.Status },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };


                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    #region BodyHardcode
                    string SalesAc = "";
                    string TaxCode = PlaceofSupply == "Dubai" ? "SR" : "ZR";
                    string RoomType = "";
                    string NoofPerson = (Convert.ToInt32(objfocus.Body[i].Adults) + Convert.ToInt32(objfocus.Body[i].Children)).ToString();
                    string guest = objfocus.Body[i].FirstName + " " + objfocus.Body[i].LastName;
                    string CutomerInfo = objfocus.Body[i].Email + ", " + objfocus.Body[i].CountryCode + ", " + objfocus.Body[i].City + ", " + objfocus.Body[i].StreetAddr + ", " + objfocus.Body[i].PostCode + ", " + objfocus.Body[i].PhoneNo + ", " + objfocus.Body[i].MobilePhone;
                    #endregion BodyHardcode 

                    if (objfocus.Body[i].Item == "")
                    {
                        Clsdata.LogFile(doctype, "Item should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsNameExist(ccode, "mCore_RoomType", objfocus.Body[i].Item, ""))
                        {
                            int a = CreateMaster(objfocus.Body[i].Item, ccode, "RoomType");
                            if (a == -1)
                            {
                                Clsdata.LogFile(doctype, "Posting of New Room Type " + objfocus.Body[i].Item + " Failed", writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Posting of New Room Type " + objfocus.Body[i].Item + " Failed";
                                return objreturn;
                            }
                            else
                            {
                                RoomType = objfocus.Body[i].Item;
                            }
                        }
                        else
                        {
                            RoomType = objfocus.Body[i].Item;

                        }
                    }
                    string vat = "";
                    string item = "";
                    string vattype = "1";

                    #region RoomRent
                    item = "12";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", NoofPerson },
                        { "Rate", objfocus.Body[i].Rate },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", guest },
                        { "Customer Info",CutomerInfo },
                        { "Meal Plan", objfocus.Body[i].MealPlan },
                        { "No of Nights", objfocus.Body[i].NoofNights },
                        { "Adults", objfocus.Body[i].Adults },
                        { "Children", objfocus.Body[i].Children },
                        { "NationalityofGuest", objfocus.Body[i].NationalityOfGuest },
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion RoomRent

                    #region ServiceCharge
                    item = "13";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].ServiceCharges },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", ""},
                        { "No of Nights", "" },
                        { "Adults", ""},
                        { "Children", ""},
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion ServiceCharge

                    #region MuncipalityCharges
                    item = "14";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].MunicipalityFee },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", "" },
                        { "No of Nights", "" },
                        { "Adults", "" },
                        { "Children", "" },
                        { "Unit__Id", 0},

                    };
                    body.Add(row);
                    #endregion MuncipalityCharges

                }
                var postingData = new ClsProperties.PostingData();
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, " SalesInvoice JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Sales Invoice NMC";
                Clsdata.LogFile(doctype, " SalesInvoice url:" + url, writelog);
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, " SalesInvoice response :" + response, writelog);
                Clsdata.LogFile(doctype, " SalesInvoice error :" + error, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    Clsdata.LogFile(doctype, " SalesInvoice responseData :" + responseData.message, writelog);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = "SalesInvoice Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region SalesReturn
        public ClsSalesInvoice.SalesResponse SalesReturn(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "SalesReturn";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create Sales Return", writelog);
                objreturn = CreateReturn(objfocus, ccode);
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreateReturn(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreateSalesReturn", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "SalesReturnResponse", writelog);
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(0, 2)));
                int checkindate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(0, 2)));
                int bookingdate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + checkindate.ToString(), writelog);
                if (objfocus.Header.CustomerName == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.CustomerName, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.CustomerName;
                    return objreturn;
                }
                else
                {
                    if (!IsNameExist(ccode, "mcore_Account", objfocus.Header.CustomerName, " and iAccountType = 5"))
                    {
                        int a = CreateMaster(objfocus.Header.CustomerName, ccode, "Account");
                        if (a == -1)
                        {
                            Clsdata.LogFile(doctype, "Posting of New Customer " + objfocus.Header.CustomerName + " Failed", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Posting of New Customer " + objfocus.Header.CustomerName + " Failed";
                            return objreturn;
                        }
                        else
                        {
                            //objfocus.Header.CustomerName = a.ToString();
                        }
                    }
                }
                Clsdata.LogFile(doctype, "objfocus.Header.Customer = " + objfocus.Header.CustomerName.ToString(), writelog);

                string strDueDt = getDueDate(objfocus.Body[0].CheckOutDate, objfocus.Header.CustomerName.ToString());
                dt = new DateTime(Convert.ToInt32(strDueDt.Substring(6, 4)), Convert.ToInt32(strDueDt.Substring(3, 2)), Convert.ToInt32(strDueDt.Substring(0, 2)));
                Clsdata.LogFile(doctype, "due date" + dt.ToString(), writelog);
                int duedate = cls.GetDateToInt(dt);
                Clsdata.LogFile(doctype, "int due date" + duedate.ToString(), writelog);

                string curQry = $@"select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}'";
                DataSet cds = Clsdata.GetData(curQry, ccode);
                Clsdata.LogFile(doctype, curQry, writelog);
                if (cds.Tables[0].Rows.Count > 0)
                {
                    objfocus.Header.CurrencyName = cds.Tables[0].Rows[0]["iCurrencyId"].ToString();
                }

                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }
                #region HardcodedArea
                string FaTag_id = "CBTD";//fixed as Cosmobeds Travel DMCC
                string PlaceofSupply = objfocus.Header.CustomerCountry.ToUpper() == "AE" ? "Dubai" : "Export";
                string Juridction = "Dubai";
                #endregion HardcodedArea

                Hashtable header = new Hashtable
                {

                    { "Date",docdate  },
                    { "CustomerAC__Name",objfocus.Header.CustomerName},
                    { "DueDate",duedate },
                    { "Company Master__Code",FaTag_id },
                    {"Place of supply__Name",PlaceofSupply },
                    {"Jurisdiction__Name",Juridction },
                    {"Transaction Type__Name","Web Booking" },
                    { "Salesman__Name", "Web" },
                    { "ExchangeRate", objfocus.Header.ExchangeRate },
                    { "Hotel Master__Name", objfocus.Header.HotelMaster },
                    { "ConfirmationNumber", objfocus.Header.ConfirmationNo },
                    { "Currency__Id",objfocus.Header.CurrencyName },
                    { "HotelCity", objfocus.Header.HotelCity },
                    { "HotelCountry", objfocus.Header.HotelCountry },
                    { "HotelPhoneNumber", objfocus.Header.HotelPhoneNumber },
                    { "SupplierConfirmationNumber", objfocus.Header.SupplierConfirmationNumber },
                    { "ClientConfirmationNumber", objfocus.Header.ClientConfirmationNumber },
                    { "AgencyVATNumber", objfocus.Header.AgencyVATNumber },
                    { "AgencyAddress", objfocus.Header.AgencyAddress },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };


                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    #region BodyHardcode
                    string SalesAc = "";
                    string TaxCode = PlaceofSupply == "Dubai" ? "SR" : "ZR";
                    string RoomType = "";
                    string NoofPerson = (Convert.ToInt32(objfocus.Body[i].Adults) + Convert.ToInt32(objfocus.Body[i].Children)).ToString();
                    string guest = objfocus.Body[i].FirstName + " " + objfocus.Body[i].LastName;
                    string CutomerInfo = objfocus.Body[i].Email + ", " + objfocus.Body[i].CountryCode + ", " + objfocus.Body[i].City + ", " + objfocus.Body[i].StreetAddr + ", " + objfocus.Body[i].PostCode + ", " + objfocus.Body[i].PhoneNo + ", " + objfocus.Body[i].MobilePhone;
                    #endregion BodyHardcode 

                    if (objfocus.Body[i].Item == "")
                    {
                        Clsdata.LogFile(doctype, "Item should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsNameExist(ccode, "mCore_RoomType", objfocus.Body[i].Item, ""))
                        {
                            int a = CreateMaster(objfocus.Body[i].Item, ccode, "RoomType");
                            if (a == -1)
                            {
                                Clsdata.LogFile(doctype, "Posting of New Room Type " + objfocus.Body[i].Item + " Failed", writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Posting of New Room Type " + objfocus.Body[i].Item + " Failed";
                                return objreturn;
                            }
                            else
                            {
                                RoomType = objfocus.Body[i].Item;
                            }
                        }
                        else
                        {
                            RoomType = objfocus.Body[i].Item;

                        }
                    }
                    string vat = "";
                    string item = "";
                    string vattype = "1";
                    #region RoomRent
                    item = "12";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", NoofPerson },
                        { "Rate", objfocus.Body[i].Rate },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", guest },
                        { "Customer Info",CutomerInfo },
                        { "Meal Plan", objfocus.Body[i].MealPlan },
                        { "No of Nights", objfocus.Body[i].NoofNights },
                        { "Adults", objfocus.Body[i].Adults },
                        { "Children", objfocus.Body[i].Children },
                        { "NationalityofGuest", objfocus.Body[i].NationalityOfGuest },
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion RoomRent

                    #region ServiceCharge
                    item = "13";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].ServiceCharges },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", ""},
                        { "No of Nights", "" },
                        { "Adults", ""},
                        { "Children", ""},
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion ServiceCharge

                    #region MuncipalityCharges
                    item = "14";
                    SalesAc = getSalesAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Sales Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Sales Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].MunicipalityFee },
                        { "VAT", vat },
                        //{ "Gross", objfocus.Body[i].Gross },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "SalesAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", "" },
                        { "No of Nights", "" },
                        { "Adults", "" },
                        { "Children", "" },
                        { "Unit__Id", 0},

                    };
                    body.Add(row);
                    #endregion MuncipalityCharges

                }
                var postingData = new ClsProperties.PostingData();
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, " SalesReturn JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Sales Return NMC";
                Clsdata.LogFile(doctype, " SalesReturn url:" + url, writelog);
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, " SalesReturn response :" + response, writelog);
                Clsdata.LogFile(doctype, " SalesReturn error :" + error, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    Clsdata.LogFile(doctype, " SalesReturn responseData :" + responseData.message, writelog);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = "SalesReturn Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Purchase
        public ClsSalesInvoice.SalesResponse Purchase(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "Purchase";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create Purchase", writelog);
                objreturn = CreatePurchase(objfocus, ccode);
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreatePurchase(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreatePurchase", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "Response", writelog);
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(0, 2)));
                int checkindate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(0, 2)));
                int bookingdate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + checkindate.ToString(), writelog);
                if (objfocus.Header.CurrencyName =="")
                {
                    Clsdata.LogFile(doctype, "Currency should not be empty " , writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Currency should not be empty " ;
                    return objreturn;
                    
                }
                else
                {
                    
                    if (objfocus.Header.CurrencyName == "AED")
                    {
                        objfocus.Header.ExchangeRate = "1";
                    }
                    else
                    {
                        string sql = $@"SET DATEFORMAT dmy
                            select xd.fRate from mCore_ExchangeRateDefinition xd 
							inner join mCore_ExchangeRate xr on xd.iExchangeRateId=xr.iExchangeRateId 
							where xd.iCurrencyNameId = (select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}') 
							and xd.iExchangeRateId = (select top(1)xd.iExchangeRateId from mCore_ExchangeRateDefinition xd 
							inner join mCore_ExchangeRate xr on xd.iExchangeRateId=xr.iExchangeRateId 
							where xd.iCurrencyNameId = (select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}') and cast(dbo.fCore_IntToDateTime(xr.iEffectiveDate) as date) <= cast('{objfocus.Body[0].CheckOutDate}' as date) order by xr.iEffectiveDate desc) SET DATEFORMAT mdy";
                        Clsdata.LogFile(doctype, sql, writelog);
                        DataSet dss = Clsdata.GetData(sql, ccode);
                        if (dss.Tables[0].Rows.Count > 0)
                        {
                            objfocus.Header.ExchangeRate = dss.Tables[0].Rows[0][0].ToString();
                        }
                        else
                        {
                            Clsdata.LogFile(doctype, "Currency Not Defined " + objfocus.Header.CurrencyName, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Currency Not Defined " + objfocus.Header.CurrencyName;
                            return objreturn;
                        }
                    }
                    string curQry = $@"select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}'";
                    DataSet cds = Clsdata.GetData(curQry, ccode);
                    Clsdata.LogFile(doctype, curQry, writelog);
                    if (cds.Tables[0].Rows.Count > 0)
                    {
                        objfocus.Header.CurrencyName = cds.Tables[0].Rows[0]["iCurrencyId"].ToString();
                    }
                }
                if (objfocus.Header.CustomerName == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.CustomerName, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.CustomerName;
                    return objreturn;
                }
                else
                {
                    if (!IsNameExist(ccode, "mcore_Account", objfocus.Header.CustomerName, " and iAccountType = 6"))
                    {
                        int a = CreateMaster(objfocus.Header.CustomerName, ccode, "Account2");
                        if (a == -1)
                        {
                            Clsdata.LogFile(doctype, "Posting of New Customer " + objfocus.Header.CustomerName + " Failed", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Posting of New Customer " + objfocus.Header.CustomerName + " Failed";
                            return objreturn;
                        }
                        else
                        {
                            //objfocus.Header.CustomerName = a.ToString();
                        }
                    }
                }

                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }
                #region HardcodedArea
                string FaTag_id = "CBTD";//fixed as Cosmobeds Travel DMCC
                string PlaceofSupply = objfocus.Header.CustomerCountry.ToUpper() == "AE" ? "Dubai" : "Import";
                string Juridction = "Dubai";
                #endregion HardcodedArea

                Hashtable header = new Hashtable
                {
                    { "Date",docdate  },
                    { "VendorAC__Name",objfocus.Header.CustomerName},
                    { "DueDate",docdate },
                    { "Company Master__Code",FaTag_id },
                    {"Place of supply__Name",PlaceofSupply },
                    {"Jurisdiction__Name",Juridction },
                    {"Transaction Type__Name","Web Booking" },
                    { "Salesman__Name", "Web" },
                    { "ExchangeRate", objfocus.Header.ExchangeRate },
                    { "Hotel Master__Name", objfocus.Header.HotelMaster },
                    { "ConfirmationNumber", objfocus.Header.ConfirmationNo },
                    { "Currency__Id",objfocus.Header.CurrencyName },
                    { "HotelCity", objfocus.Header.HotelCity },
                    { "HotelCountry", objfocus.Header.HotelCountry },
                    { "HotelPhoneNumber", objfocus.Header.HotelPhoneNumber },
                    { "SupplierConfirmationNumber", objfocus.Header.SupplierConfirmationNumber },
                    { "ClientConfirmationNumber", objfocus.Header.ClientConfirmationNumber },
                    { "AgencyVATNumber", objfocus.Header.AgencyVATNumber },
                    { "AgencyAddress", objfocus.Header.AgencyAddress },
                    { "Status__Name", objfocus.Header.Status },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };


                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    #region BodyHardcode
                    string SalesAc = "";
                    string TaxCode = PlaceofSupply == "Dubai" ? "SR-REC" : "OS";
                    string RoomType = "";
                    string NoofPerson = (Convert.ToInt32(objfocus.Body[i].Adults) + Convert.ToInt32(objfocus.Body[i].Children)).ToString();
                    string guest = objfocus.Body[i].FirstName + " " + objfocus.Body[i].LastName;
                    string CutomerInfo = objfocus.Body[i].Email + ", " + objfocus.Body[i].CountryCode + ", " + objfocus.Body[i].City + ", " + objfocus.Body[i].StreetAddr + ", " + objfocus.Body[i].PostCode + ", " + objfocus.Body[i].PhoneNo + ", " + objfocus.Body[i].MobilePhone;
                    #endregion BodyHardcode 

                    if (objfocus.Body[i].Item == "")
                    {
                        Clsdata.LogFile(doctype, "Item should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsNameExist(ccode, "mCore_RoomType", objfocus.Body[i].Item, ""))
                        {
                            int a = CreateMaster(objfocus.Body[i].Item, ccode, "RoomType");
                            if (a == -1)
                            {
                                Clsdata.LogFile(doctype, "Posting of New Room Type " + objfocus.Body[i].Item + " Failed", writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Posting of New Room Type " + objfocus.Body[i].Item + " Failed";
                                return objreturn;
                            }
                            else
                            {
                                RoomType = objfocus.Body[i].Item;
                            }
                        }
                        else
                        {
                            RoomType = objfocus.Body[i].Item;

                        }
                    }
                    string vat = "";
                    string item = "";
                    string vattype = "0";

                    #region RoomRent
                    item = "12";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", NoofPerson },
                        { "Rate", objfocus.Body[i].Rate },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].Rate)* Convert.ToDecimal(objfocus.Body[i].Quantity) },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", guest },
                        { "Customer Info",CutomerInfo },
                        { "Meal Plan", objfocus.Body[i].MealPlan },
                        { "No of Nights", objfocus.Body[i].NoofNights },
                        { "Adults", objfocus.Body[i].Adults },
                        { "Children", objfocus.Body[i].Children },
                        { "NationalityofGuest", objfocus.Body[i].NationalityOfGuest },
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion RoomRent

                    #region ServiceCharge
                    item = "13";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].ServiceCharges },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].ServiceCharges)* Convert.ToDecimal(objfocus.Body[i].Quantity) },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", ""},
                        { "No of Nights", "" },
                        { "Adults", ""},
                        { "Children", ""},
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion ServiceCharge

                    #region MuncipalityCharges
                    item = "14";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].MunicipalityFee },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].MunicipalityFee)* Convert.ToDecimal(objfocus.Body[i].Quantity) },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", "" },
                        { "No of Nights", "" },
                        { "Adults", "" },
                        { "Children", "" },
                        { "Unit__Id", 0},

                    };
                    body.Add(row);
                    #endregion MuncipalityCharges

                }
                var postingData = new ClsProperties.PostingData();
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, doctype+" JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Purchase NMC";
                Clsdata.LogFile(doctype, doctype + " url:" + url, writelog);
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, doctype + " response :" + response, writelog);
                Clsdata.LogFile(doctype, doctype + "error :" + error, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    Clsdata.LogFile(doctype, doctype + " responseData :" + responseData.message, writelog);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = doctype + " Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region PurchaseReturn
        public ClsSalesInvoice.SalesResponse PurchaseReturn(ClsSalesInvoice.SalesInvoice objfocus)
        {
            doctype = "PurchaseReturn";
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            MessageHeaders headers = OperationContext.Current.IncomingMessageHeaders;
            Clsdata.LogFile(doctype, " Posting in to Focus", writelog);

            try
            {
                if (headers.To.Query != "")
                {
                    var data = headers.To.Query.Substring(1);
                    if (data != "")
                    {
                        AccessToken = Convert.ToString(data.Split('=')[1]);
                    }

                }
                string Token = AccessToken;
                if (Token == "")
                {
                    Clsdata.LogFile(doctype, "Invalid Token", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Should not be Empty";

                    return objreturn;
                }
                string AuthToken = getToken(doctype, Token);
                Token = AuthToken;
                AuthenticationModule objauth = new AuthenticationModule();
                var ret = objauth.GenerateUserClaimFromJWT(Token);
                //if ret.Payload.
                if (ret == null)
                {
                    Clsdata.LogFile(doctype, " Token Expired", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Token Expired";

                    return objreturn;
                }
                ccode = cls.GetCompanyId(companycode);
                Clsdata.LogFile(doctype, "CompanyCode = " + ccode.ToString(), writelog);
                Clsdata.LogFile(doctype, "Go To Create PurchaseReturn", writelog);
                objreturn = CreatePurchaseReturn(objfocus, ccode);
            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = ex.Message;
            }
            return objreturn;
        }
        public ClsSalesInvoice.SalesResponse CreatePurchaseReturn(ClsSalesInvoice.SalesInvoice objfocus, int ccode)
        {
            Clsdata.LogFile(doctype, "Entered CreatePurchaseReturn", writelog);
            ClsSalesInvoice.SalesResponse objreturn = new ClsSalesInvoice.SalesResponse();
            Clsdata.LogFile(doctype, "Response", writelog);
            try
            {
                DateTime dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckOutDate.Substring(0, 2)));
                Clsdata.LogFile(doctype, "date" + dt.ToString(), writelog);
                int docdate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].CheckInDate.Substring(0, 2)));
                int checkindate = cls.GetDateToInt(dt);

                dt = new DateTime(Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(6, 4)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(3, 2)), Convert.ToInt32(objfocus.Body[0].BookingDate.Substring(0, 2)));
                int bookingdate = cls.GetDateToInt(dt);
                //int docdate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "docdate" + docdate.ToString(), writelog);
                //int duedate = cls.GetDateToInt(Convert.ToDateTime(objfocus.Header.Date));
                Clsdata.LogFile(doctype, "duedate" + checkindate.ToString(), writelog);
                if (objfocus.Header.CurrencyName == "")
                {
                    Clsdata.LogFile(doctype, "Currency should not be empty ", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Currency should not be empty ";
                    return objreturn;

                }
                else
                {
                    if (objfocus.Header.CurrencyName == "AED")
                    {
                        objfocus.Header.ExchangeRate = "1";
                    }
                    else
                    {
                        string sql = $@"SET DATEFORMAT dmy
                            select xd.fRate from mCore_ExchangeRateDefinition xd 
							inner join mCore_ExchangeRate xr on xd.iExchangeRateId=xr.iExchangeRateId 
							where xd.iCurrencyNameId = (select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}') 
							and xd.iExchangeRateId = (select top(1)xd.iExchangeRateId from mCore_ExchangeRateDefinition xd 
							inner join mCore_ExchangeRate xr on xd.iExchangeRateId=xr.iExchangeRateId 
							where xd.iCurrencyNameId = (select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}') and cast(dbo.fCore_IntToDateTime(xr.iEffectiveDate) as date) <= cast('{objfocus.Body[0].CheckOutDate}' as date) order by xr.iEffectiveDate desc) SET DATEFORMAT mdy";
                        Clsdata.LogFile(doctype, sql, writelog);
                        DataSet dss = Clsdata.GetData(sql, ccode);
                        if (dss.Tables[0].Rows.Count > 0)
                        {
                            objfocus.Header.ExchangeRate = dss.Tables[0].Rows[0][0].ToString();
                        }
                        else
                        {
                            Clsdata.LogFile(doctype, "Currency Not Defined " + objfocus.Header.CurrencyName, writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Currency Not Defined " + objfocus.Header.CurrencyName;
                            return objreturn;
                        }
                    }
                    string curQry = $@"select iCurrencyId from mCore_Currency where sCode ='{objfocus.Header.CurrencyName}'";
                    DataSet cds = Clsdata.GetData(curQry, ccode);
                    Clsdata.LogFile(doctype, curQry, writelog);
                    if (cds.Tables[0].Rows.Count > 0)
                    {
                        objfocus.Header.CurrencyName = cds.Tables[0].Rows[0]["iCurrencyId"].ToString();
                    }
                }
                if (objfocus.Header.CustomerName == "")
                {
                    Clsdata.LogFile(doctype, "Customer should not be empty" + objfocus.Header.CustomerName, writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Customer Account should not be empty" + objfocus.Header.CustomerName;
                    return objreturn;
                }
                else
                {
                    if (!IsNameExist(ccode, "mcore_Account", objfocus.Header.CustomerName, " and iAccountType = 6"))
                    {
                        int a = CreateMaster(objfocus.Header.CustomerName, ccode, "Account2");
                        if (a == -1)
                        {
                            Clsdata.LogFile(doctype, "Posting of New Customer " + objfocus.Header.CustomerName + " Failed", writelog);
                            objreturn.iStatus = 0;
                            objreturn.sMessage = "Posting of New Customer " + objfocus.Header.CustomerName + " Failed";
                            return objreturn;
                        }
                        else
                        {
                            //objfocus.Header.CustomerName = a.ToString();
                        }
                    }
                }

                string InvTag = "";
                string FinTag = "";
                string Tags = "";
                Tags = GetTagName(ccode);
                if (Tags == "")
                {
                    Clsdata.LogFile(doctype, "Error in getting Inventory Tag and Financial Tag", writelog);
                    objreturn.iStatus = 0;
                    objreturn.sMessage = "Error in getting Inventory Tag and Financial Tag ";
                    return objreturn;
                }
                else
                {
                    InvTag = Tags.Split(',')[0];
                    Clsdata.LogFile(doctype, "InvTag" + InvTag, writelog);
                    FinTag = Tags.Split(',')[1];
                    Clsdata.LogFile(doctype, "FinTag" + FinTag, writelog);
                }
                #region HardcodedArea
                string FaTag_id = "CBTD";//fixed as Cosmobeds Travel DMCC
                string PlaceofSupply = objfocus.Header.CustomerCountry.ToUpper() == "AE" ? "Dubai" : "Import";
                string Juridction = "Dubai";
                #endregion HardcodedArea

                Hashtable header = new Hashtable
                {
                    { "Date",docdate  },
                    { "VendorAC__Name",objfocus.Header.CustomerName},
                    { "DueDate",docdate },
                    { "Company Master__Code",FaTag_id },
                    {"Place of supply__Name",PlaceofSupply },
                    {"Jurisdiction__Name",Juridction },
                    {"Transaction Type__Name","Web Booking" },
                    { "Salesman__Name", "Web" },
                    { "ExchangeRate", objfocus.Header.ExchangeRate },
                    { "Hotel Master__Name", objfocus.Header.HotelMaster },
                    { "ConfirmationNumber", objfocus.Header.ConfirmationNo },
                    { "Currency__Id",objfocus.Header.CurrencyName },
                    { "HotelCity", objfocus.Header.HotelCity },
                    { "HotelCountry", objfocus.Header.HotelCountry },
                    { "HotelPhoneNumber", objfocus.Header.HotelPhoneNumber },
                    { "SupplierConfirmationNumber", objfocus.Header.SupplierConfirmationNumber },
                    { "ClientConfirmationNumber", objfocus.Header.ClientConfirmationNumber },
                    { "AgencyVATNumber", objfocus.Header.AgencyVATNumber },
                    { "AgencyAddress", objfocus.Header.AgencyAddress },
            };

                List<Hashtable> body = new List<Hashtable>();
                Hashtable row = new Hashtable { };


                for (int i = 0; i < objfocus.Body.Count; i++)
                {
                    #region BodyHardcode
                    string SalesAc = "";
                    string TaxCode = PlaceofSupply == "Dubai" ? "SR-REC" : "OS";
                    string RoomType = "";
                    string NoofPerson = (Convert.ToInt32(objfocus.Body[i].Adults) + Convert.ToInt32(objfocus.Body[i].Children)).ToString();
                    string guest = objfocus.Body[i].FirstName + " " + objfocus.Body[i].LastName;
                    string CutomerInfo = objfocus.Body[i].Email + ", " + objfocus.Body[i].CountryCode + ", " + objfocus.Body[i].City + ", " + objfocus.Body[i].StreetAddr + ", " + objfocus.Body[i].PostCode + ", " + objfocus.Body[i].PhoneNo + ", " + objfocus.Body[i].MobilePhone;
                    #endregion BodyHardcode 

                    if (objfocus.Body[i].Item == "")
                    {
                        Clsdata.LogFile(doctype, "Item should not be empty", writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Item should not be empty";
                        return objreturn;
                    }
                    else
                    {
                        if (!IsNameExist(ccode, "mCore_RoomType", objfocus.Body[i].Item, ""))
                        {
                            int a = CreateMaster(objfocus.Body[i].Item, ccode, "RoomType");
                            if (a == -1)
                            {
                                Clsdata.LogFile(doctype, "Posting of New Room Type " + objfocus.Body[i].Item + " Failed", writelog);
                                objreturn.iStatus = 0;
                                objreturn.sMessage = "Posting of New Room Type " + objfocus.Body[i].Item + " Failed";
                                return objreturn;
                            }
                            else
                            {
                                RoomType = objfocus.Body[i].Item;
                            }
                        }
                        else
                        {
                            RoomType = objfocus.Body[i].Item;
                        }
                    }
                    string vat = "";
                    string item = "";
                    string vattype = "0";

                    #region RoomRent
                    item = "12";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", NoofPerson },
                        { "Rate", objfocus.Body[i].Rate },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].Rate)* Convert.ToDecimal(objfocus.Body[i].Quantity) },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", guest },
                        { "Customer Info",CutomerInfo },
                        { "Meal Plan", objfocus.Body[i].MealPlan },
                        { "No of Nights", objfocus.Body[i].NoofNights },
                        { "Adults", objfocus.Body[i].Adults },
                        { "Children", objfocus.Body[i].Children },
                        { "NationalityofGuest", objfocus.Body[i].NationalityOfGuest },
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion RoomRent

                    #region ServiceCharge
                    item = "13";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", 1 },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].ServiceCharges },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].ServiceCharges)* Convert.ToDecimal(objfocus.Body[i].Quantity)},
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", ""},
                        { "No of Nights", "" },
                        { "Adults", ""},
                        { "Children", ""},
                        { "Unit__Id", 0},
                    };
                    body.Add(row);
                    #endregion ServiceCharge

                    #region MuncipalityCharges
                    item = "14";
                    SalesAc = getPurchaseAc(item);
                    if (SalesAc == "")
                    {
                        Clsdata.LogFile(doctype, "Purchase Account is not mapped with Item in focus  " + SalesAc, writelog);
                        objreturn.iStatus = 0;
                        objreturn.sMessage = "Purchase Account is not mapped with Item in focus" + SalesAc;
                        return objreturn;
                    }
                    vat = getVat(PlaceofSupply, Juridction, item, vattype);
                    row = new Hashtable
                    {
                        { "Item",item },
                        { "Room Type__Name", RoomType },
                        { "TaxCode__Code",TaxCode },
                        { "Description", objfocus.Body[i].Item },
                        { "Quantity", objfocus.Body[i].Quantity },
                        { "No of Person", "" },
                        { "Rate", objfocus.Body[i].MunicipalityFee },
                        { "VAT", vat },
                        { "Gross", Convert.ToDecimal(objfocus.Body[i].MunicipalityFee)* Convert.ToDecimal(objfocus.Body[i].Quantity) },
                        { "Check In Date", checkindate },
                        { "Check Out Date", docdate },
                        { "PurchaseAC__Id", SalesAc },
                        { "Booking Date",bookingdate},
                        { "Guest Name", "" },
                        { "Customer Info","" },
                        { "Meal Plan", "" },
                        { "No of Nights", "" },
                        { "Adults", "" },
                        { "Children", "" },
                        { "Unit__Id", 0},

                    };
                    body.Add(row);
                    #endregion MuncipalityCharges

                }
                var postingData = new ClsProperties.PostingData();
                postingData.data.Add(new Hashtable { { "Header", header }, { "Body", body } });
                string sContent = JsonConvert.SerializeObject(postingData);
                Clsdata.LogFile(doctype, doctype + " JSon:" + sContent, writelog);
                string err = "";
                sessionId = getsessionid(username, Password, companycode);
                string url = "http://" + serverip + "/Focus8API/Transactions/Vouchers/Purchase Return NMC";
                Clsdata.LogFile(doctype, doctype + " url:" + url, writelog);
                string error = "";
                var response = Post(url, sContent, sessionId, ref error);

                Clsdata.LogFile(doctype, doctype + " response :" + response, writelog);
                Clsdata.LogFile(doctype, doctype + "error :" + error, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    Clsdata.LogFile(doctype, doctype + " responseData :" + responseData.message, writelog);
                    if (responseData.result != -1)
                    {
                        objreturn.sVouchrNo = Convert.ToString(responseData.data[0]["VoucherNo"]);
                        objreturn.iStatus = 1;


                        objreturn.sMessage = doctype + " Posted Successfully";
                        bool flg = logout(sessionId, serverip);
                    }
                    else
                    {
                        objreturn.iStatus = 0;
                        objreturn.sMessage = responseData.message;
                        bool flg = logout(sessionId, serverip);
                    }
                }

            }
            catch (Exception ex)
            {
                objreturn.iStatus = 0;
                objreturn.sMessage = "Exception" + ex.Message;
            }

            return objreturn;
        }
        #endregion

        #region Common Methods
        public static string Post(string url, string data, string sessionId, ref string err)
        {
            try
            {
                using (var client = new WebClient())
                {
                    ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
                    client.Encoding = Encoding.UTF8;
                    client.Timeout = 30 * 60 * 1000;
                    client.Headers.Add("fSessionId", sessionId);
                    client.Headers.Add("Content-Type", "application/json");
                    var response = client.UploadString(url, data);
                    return response;
                }
            }
            catch (Exception e)
            {
                err = e.Message;
                return null;
            }

        }
        public bool logout(string sessionid, string serverip)
        {
            bool flg = false;
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("fsessionid", sessionid);
                client.Timeout = 10 * 60 * 1000;
                client.Headers.Add("Content-Type", "application/json");
                var arrResponse = client.DownloadString("http://" + serverip + "/focus8API/Logout");
                flg = true;
            }
            return flg;
        }
        public string getsessionid(string usrename, string password, string companycode)
        {
            string sid = "";
            ClsProperties.Datum datanum = new ClsProperties.Datum();
            datanum.CompanyCode = companycode;
            datanum.Username = usrename;
            datanum.password = password;
            List<ClsProperties.Datum> lstd = new List<ClsProperties.Datum>();
            lstd.Add(datanum);
            ClsProperties.Lolgin lngdata = new ClsProperties.Lolgin();
            lngdata.data = lstd;
            string sContent = JsonConvert.SerializeObject(lngdata);
            using (WebClient client = new WebClient())
            {
                client.Headers.Add("Content-Type", "application/json");
                client.Timeout = 10 * 60 * 1000;
                var arrResponse = client.UploadString("http://" + serverip + "/focus8API/Login", sContent);
                //returnObject = new clsDeserialize().Deserialize<RootObject>(arrResponse);
                ClsProperties.Resultlogin lng = JsonConvert.DeserializeObject<ClsProperties.Resultlogin>(arrResponse);

                sid = lng.data[0].fSessionId;


            }

            return sid;
        }
        public int Getid(string name, int ccode, string tablename, int type)
        {
            int id = 0;
            DataSet dss = new DataSet();
            string qry = "";
            if (type == 0)
            {
                qry = "select imasterid from  " + tablename + "   where sname ='" + name + "' and istatus<>5 and bgroup=0  ";
                dss = Clsdata.GetData(qry, ccode);

            }
            else
            {
                qry = "select imasterid from  " + tablename + "  where scode ='" + name + "' and istatus<>5 and bgroup=0  ";
                dss = Clsdata.GetData(qry, ccode);
            }
            Clsdata.LogFile(doctype, qry, writelog);
            if (dss.Tables[0].Rows.Count > 0)
            {
                id = Convert.ToInt32(dss.Tables[0].Rows[0]["imasterid"]);
            }

            return id;
        }
        public string GetTagName(int ccode)
        {
            string TagName = "";
            try
            {
                string qry = "select(SELECT sMasterName  FROM cCore_MasterDef WHERE iMasterTypeId = (SELECT iValue FROM cCore_PreferenceVal_0 WHERE iCategory = 0 and iFieldId = 1)) Invtag,(SELECT sMasterName FROM cCore_MasterDef WHERE iMasterTypeId = (SELECT iValue FROM cCore_PreferenceVal_0 WHERE iCategory = 0 and iFieldId = 0)) FinTag";
                Clsdata.LogFile(doctype, qry, writelog);
                DataSet dss = Clsdata.GetData(qry, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    TagName = dss.Tables[0].Rows[0]["Invtag"].ToString() + "," + dss.Tables[0].Rows[0]["FinTag"].ToString();
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return TagName;
        }
        public string GetExtraFeild(string TagName, int ccode, string TableName, string masterid)
        {
            string TagValue = "";
            try
            {
                string sql = "select " + TagName + " from " + TableName + " where iMasterId = " + masterid;
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    TagValue = dss.Tables[0].Rows[0][0].ToString();
                    Clsdata.LogFile(doctype, "TagValue = " + TagValue, writelog);
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return TagValue;
        }
        public bool IsExist(int ccode, string TableName, string val)
        {
            bool IsExists = false;
            try
            {
                string sql = "select 1 from " + TableName + " where iMasterId = " + val;
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    IsExists = true;
                }
                else
                {
                    IsExists = false;
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return IsExists;
        }
        public bool IsNameExist(int ccode, string TableName, string val, string filter)
        {
            bool IsExists = false;
            try
            {
                string sql = $@"select 1 from {TableName} where sName = '{val}' {filter}";
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    IsExists = true;
                }
                else
                {
                    IsExists = false;
                }
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(doctype, ex.Message, writelog);
            }
            return IsExists;
        }
        public DataSet GetReturnDSTranID(string DocNo, int ID, string BatchNo, int Type)
        {
            #region Default Acc

            int LinkPathId = 0;
            int InvVchr = 0;
            string sql = "";

            string Lsql = $@"declare @inv int;
                            declare @ret int;
                            set @inv = (select iVoucherType from cCore_Vouchers_0 where sName = 'Sales Invoice - VAN');
                            set @ret = (select iVoucherType from cCore_Vouchers_0 where sName = 'Sales Return - VAN');
                            select ilinkpathid,@inv invtype,@ret rettype from vmCore_Links_0 with (ReadUnCommitted)  where BaseVoucherId=@inv and LinkVoucherId=@ret group by ilinkpathid,Basevoucherid";
            DataSet lds = Clsdata.GetData(Lsql, ccode);
            for (int i = 0; i < lds.Tables[0].Rows.Count; i++)
            {
                LinkPathId = Convert.ToInt32(lds.Tables[0].Rows[i]["ilinkpathid"]);
                InvVchr = Convert.ToInt32(lds.Tables[0].Rows[i]["invtype"]);
            }

            sql = $@"select svoucherno,iLinkId, (fvalue-linkvalue)penvalue,iproduct,ibodyid from  (select h.svoucherno,iLinkId,fvalue,
                    i.iproduct,i.ibodyid, (select isnull(sum(fvalue),0) from tcore_links_0 tl1 where  tl1.bbase=0 
                    and tl1.ilinkid=tl.ilinkid and tl1.irefid=tl.itransactionid)linkvalue from tcore_header_0 h with (ReadUnCommitted) 
                    join tcore_data_0 d with (ReadUnCommitted) on d.iheaderid=h.iheaderid  join tcore_indta_0 i with (ReadUnCommitted) on i.ibodyid=d.ibodyid
                    join tcore_links_0 tl with (ReadUnCommitted) on tl.itransactionid=d.itransactionid  
                    join tcore_headerdata{InvVchr}_0 uh with (ReadUnCommitted) on uh.iheaderid=d.iheaderid 
                    join tcore_data{InvVchr}_0 ub with (ReadUnCommitted) on ub.ibodyid=d.ibodyid where tl.ilinkid={LinkPathId}
                    and tl.bbase=1 and h.bsuspended=0  and h.sVoucherNo='{DocNo}' and iProduct={ID}
                    )a where (fvalue-linkvalue)>0";
            Clsdata.LogFile(doctype, "sql = " + sql, writelog);
            DataSet ds = Clsdata.GetData(sql, ccode);
            return ds;
            #endregion
        }
        public DataSet GetIRefDs(string RefNo)
        {
            #region Default Acc

            string sql = $@"EXEC Proc_LN_Vnd_Blk_Pnd '{RefNo}'";
            Clsdata.LogFile(doctype, "sql = " + sql, writelog);
            DataSet ds = Clsdata.GetData(sql, ccode);
            return ds;
            #endregion
        }
        public string getSalesAc(string masterid)
        {
            try
            {
                string sql = "select dbo.GetProductSalesAc(" + Convert.ToInt32(masterid) + ")";
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    return dss.Tables[0].Rows[0][0].ToString();
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string getPurchaseAc(string masterid)
        {
            try
            {
                string sql = "select dbo.GetProductPurchaseAc(" + Convert.ToInt32(masterid) + ")";
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    return dss.Tables[0].Rows[0][0].ToString();
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public string getVat(string placeofSupply, string Juridiction, string masterid,string type)
        {
            try
            {
                string sql = $@"select t.fPerc from mCore_TaxRate t
                join mCore_PlaceOfSupply p on p.iMasterId = t.iPlaceSupply
                join mCore_Jurisdiction j on j.iMasterId = t.iJuris
                join muCore_Product_Settings s on s.TaxCategory = t.iTaxCat
                where p.sName = '{placeofSupply}' and j.sName = '{Juridiction}' and s.iMasterId = {masterid} and iType = {type}";
                Clsdata.LogFile(doctype, "sql = " + sql, writelog);
                DataSet dss = Clsdata.GetData(sql, ccode);
                if (dss.Tables[0].Rows.Count > 0)
                {
                    return dss.Tables[0].Rows[0][0].ToString();
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
        }
        public int CreateMaster(string name, int ccode, string tablename)
        {
            int id = 0;
            int count = 0;
            try
            {
                Hashtable objHash1 = new Hashtable();
                if (tablename == "Account")
                {
                    objHash1.Add("sName", name);
                    objHash1.Add("sCode", name);
                    objHash1.Add("iAccountType", 5);
                    objHash1.Add("iParentId", 125);
                }
                else if (tablename == "Account2")
                {
                    objHash1.Add("sName", name);
                    objHash1.Add("sCode", name);
                    objHash1.Add("iAccountType", 6);
                    objHash1.Add("iParentId", 117);
                    tablename = "Account";
                }
                else
                {
                    objHash1.Add("sName", name);
                    objHash1.Add("sCode", name);
                }
                string errText = "";
                var postingData1 = new PostingData();
                postingData1.data.Add(objHash1);
                string sContent = JsonConvert.SerializeObject(postingData1);
                Clsdata.LogFile("Master", sContent, writelog);
                string baseUrl = "http://" + serverip + "/Focus8API/Masters/Core__" + tablename + "";
                Clsdata.LogFile("Master", baseUrl, writelog);
                sessionId = getsessionid(username, Password, companycode);
                Clsdata.LogFile("Master", sessionId, writelog);
                string response = Post(baseUrl, sContent, sessionId, ref errText);
                Clsdata.LogFile("Master", response, writelog);
                if (response != null)
                {
                    var responseData = JsonConvert.DeserializeObject<ClsProperties.PostResponse>(response);
                    Clsdata.LogFile("Master", responseData.message, writelog);
                    if (responseData.result == -1)
                    {

                        Clsdata.LogFile("Master", tablename + "Posting Failed \n" + errText, writelog);
                        Clsdata.LogFile("Master", tablename + "Posting Failed \n" + responseData.message, writelog);
                    }
                    else
                    {
                        Clsdata.LogFile("Master", tablename + "Posting Success ", writelog);
                    }
                }
                else
                {
                    Clsdata.LogFile("Master", tablename + "Posting Failed \n" + errText, writelog);
                    Clsdata.LogFile("Master", tablename + "Posting Failed \n" + response, writelog);
                }
            }
            catch (Exception ex)
            {
                id = -1;
                Clsdata.LogFile("Master", ex.Message, writelog);
            }

            return id;
        }
        public string getDueDate(string _dueDt, string _acID)
        {
            string duedt = _dueDt;
            DataSet dss = new DataSet();
            string qry = "";
            qry = $@"declare @days int = (select iCreditDays from mCore_Account where sName = '{_acID}' and iStatus <>5 and iMasterId<>0)
            if(@days>0)
            begin
            set @days = @days -1;
            SET DATEFORMAT dmy SELECT cast(DATEADD(day, @days,DATEADD(mm, DATEDIFF(mm, 0, '{_dueDt}') + 1, 0)) as date) SET DATEFORMAT mdy
            end
            else
            begin
            select '{_dueDt}'
            end";
            dss = Clsdata.GetData(qry, ccode);
            Clsdata.LogFile(doctype, qry, writelog);
            if (dss.Tables[0].Rows.Count > 0)
            {
                duedt = dss.Tables[0].Rows[0][0].ToString();
            }
            Clsdata.LogFile(doctype, duedt, writelog);
            return duedt;
        }
        #endregion

        #region AuthToken
        public string getToken(string Vtype, string Token)
        {
            string AuthToken = "";

            try
            {
                string username = Token.Split(',')[0].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "username = " + username, writelog);
                string Pwd = Token.Split(',')[1].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "Pwd = " + Pwd, writelog);
                string CompCode = Token.Split(',')[2].Trim().Split(':')[1].Trim();
                Clsdata.LogFile(Vtype, "CompCode = " + CompCode, writelog);
                ClsProperties.LogingResult _login = new ClsProperties.LogingResult();
                _login = Getlogin(username, Pwd, CompCode);
                AuthToken = _login.Auth_Token;
                Clsdata.LogFile(Vtype, "AuthToken = " + AuthToken, writelog);
            }
            catch (Exception ex)
            {
                Clsdata.LogFile(Vtype, ex.Message, writelog);
            }
            return AuthToken;
        }
        #endregion
    }
    #region JWT Token Generation
    public class AuthenticationModule
    {
        private const string communicationKey = "##%%12R8*O34*S89**M5687HRVST*INSevenHarvest****%%##";
        System.IdentityModel.Tokens.SecurityKey signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(communicationKey));


        // The Method is used to generate token for user
        public string GenerateTokenForUser(string userName, int userId)
        {
            var signingKey = new InMemorySymmetricSecurityKey(Encoding.UTF8.GetBytes(communicationKey));
            var now = DateTime.Now;
            var signingCredentials = new System.IdentityModel.Tokens.SigningCredentials(signingKey,
               System.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature, System.IdentityModel.Tokens.SecurityAlgorithms.Sha256Digest);

            var claimsIdentity = new ClaimsIdentity(new List<Claim>()
            {
                new Claim(ClaimTypes.Name, userName),
                new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            }, "Custom");

            var securityTokenDescriptor = new System.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                AppliesToAddress = "http://www.Focussoftnet.com",
                TokenIssuerName = "Focus",
                Subject = claimsIdentity,
                SigningCredentials = signingCredentials,
                Lifetime = new Lifetime(now, now.AddMinutes(60)),
            };


            var tokenHandler = new JwtSecurityTokenHandler();

            var plainToken = tokenHandler.CreateToken(securityTokenDescriptor);
            var signedAndEncodedToken = tokenHandler.WriteToken(plainToken);

            return signedAndEncodedToken;

        }

        /// Using the same key used for signing token, user payload is generated back
        public JwtSecurityToken GenerateUserClaimFromJWT(string authToken)
        {

            var tokenValidationParameters = new System.IdentityModel.Tokens.TokenValidationParameters()
            {
                ValidAudiences = new string[]
                      {
                    "http://www.Focussoftnet.com",
                      },

                ValidIssuers = new string[]
                  {
                      "Focus",
                  },
                IssuerSigningKey = signingKey
            };
            var tokenHandler = new JwtSecurityTokenHandler();

            System.IdentityModel.Tokens.SecurityToken validatedToken;

            try
            {

                tokenHandler.ValidateToken(authToken, tokenValidationParameters, out validatedToken);
            }
            catch (Exception ex)
            {

                return null;

            }

            return validatedToken as JwtSecurityToken;

        }

        public JWTAuthenticationIdentity PopulateUserIdentity(JwtSecurityToken userPayloadToken)
        {
            string name = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "unique_name").Value;
            string userId = ((userPayloadToken)).Claims.FirstOrDefault(m => m.Type == "nameid").Value;
            return new JWTAuthenticationIdentity(name) { UserId = Convert.ToInt32(userId), UserName = name };

        }

        public class JWTAuthenticationIdentity : GenericIdentity
        {

            public string UserName { get; set; }
            public int UserId { get; set; }
            public JWTAuthenticationIdentity(string userName)
                : base(userName)
            {
                UserName = userName;
            }
        }
    }
    #endregion


    public class WebClient : System.Net.WebClient
    {
        public int Timeout { get; set; }
        protected override WebRequest GetWebRequest(Uri uri)
        {
            WebRequest lWebRequest = base.GetWebRequest(uri);
            lWebRequest.Timeout = Timeout;
            ((HttpWebRequest)lWebRequest).ReadWriteTimeout = Timeout;
            return lWebRequest;
        }
    }
}
