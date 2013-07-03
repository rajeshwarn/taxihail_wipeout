@REM Generate WSDL
 
set namespace=MK.Booking.PayPal
set wsdl=https://www.paypalobjects.com/wsdl/PayPalSvc.wsdl
"C:\Program Files (x86)\Microsoft SDKs\Windows\v8.0A\bin\NETFX 4.0 Tools\wsdl" %wsdl% /n:%namespace%

@ECHO 
@ECHO ===============================================================
@ECHO The generated class cannot be used as is. Read the README file!
@ECHO ===============================================================
@ECHO