# WAYA-PAY-CHAT-2.0-.NET-LIBRARY
Download the .net plugin and add to project
Create a new instance of WayaPayTransaction and pass merchantId , publickey and isproduction = > new WayaPayTransaction(merchantId , publicKey , false)
call initilize to generate a Wayapay URL to process transaction
redirect to the URL
Call verify payment after getting a response to determine transaction status.
