BaseURl: http://localhost/PrjNMC_Inbound/FocusService.svc

Note: change the Dbconfig file in publish file -->xmlfiles folder
2. url  localhost place replace with IP address.


SalesInvoice
===========

Url: http://localhost/PrjNMC_Inbound/FocusService.svc/SalesInvoice?Auth_Token=Username:WebBooking,password:web@2021,CompanyCode:0O0
Url: http://localhost/PrjNMC_Inbound/FocusService.svc/SalesReturn?Auth_Token=Username:WebBooking,password:web@2021,CompanyCode:0O0

Url: http://localhost/PrjNMC_Inbound/FocusService.svc/Purchase?Auth_Token=Username:WebBooking,password:web@2021,CompanyCode:0O0

Url: http://localhost/PrjNMC_Inbound/FocusService.svc/PurchaseReturn?Auth_Token=Username:WebBooking,password:web@2021,CompanyCode:0O0



Request:
--------

{
    "Body": [
        {
            "Item": "Superior Room - Sea view1",
            "NoOfPerson": "2",
            "Quantity": "1",
            "Rate": "100.0",
            "ServiceCharges": "10",
            "MunicipalityFee": "7",
            "VAT": "5.5",
            "Gross": "122.5",
            "CheckInDate": "24-06-2021",
            "CheckOutDate": "26-06-2021",
            "BookingDate": "29-04-2021",
            "FirstName": "JOHN",
            "LastName": "SMITH",
            "Email": "reservations@cosmobeds.com",
            "CountryCode": "GR",
            "City": "Athens",
            "StreetAddr": "test",
            "PostCode": "12345",
            "PhoneNo": "12345",
            "MobilePhone": "Not Defined",
            "MealPlan": "A101220MI^42846",
            "NoofNights": "2",
            "Adults": "2",
            "Children": "0",
            "NationalityOfGuest": "Germany"
        }
    ],
    "Header": {
        "CustomerCountry": "UAE",
        "CustomerName": "TBO12",
        "CurrencyName": "AED",
        "HotelMaster": "Demo hotel",
        "ConfirmationNo": "CSM25131",
        "SupplierCode": "ASCASAZX2342342",
        "HotelCity": "Athens",
        "HotelCountry": "GR",
        "Exchange_Rate": "1.00",
        "HotelPhoneNumber": "123",
        "SupplierConfirmtionNumber": "123",
        "ClientConfirmationNumber": "123",
        "AgencyVATNumber": "123",
        "AgencyAddress": "sharjah"
 "Status": "Confirmed"
    }
}

Response:
---------
{"iStatus":1,"sMessage":"SalesInvoice Posted Successfully","sVouchrNo":"1"}

error Response:
--------------
{"iStatus":0,"sMessage":"Token Expired","sVouchrNo":null}

