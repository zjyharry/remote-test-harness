ATTENTION:
please make sure the port 8080 and 8081 is available now, or it will crash
please make sure that the run.bat have admin privillage or it will crash

Demo instructions:
please modify the port number in the textbox after port if the 8082 is already in use
the name of the test and the version will only like a personal imformation and will not be used in testharness and repository
the datetime is only required by query, after sending the request the client will give you the date time of the request, you can use that or just use the existing one (11_21_2016_10_47_08)
please DO NOT change the test author, or you will need to modify the test request

when doing link
make sure the port is not used, then click link

when sending test request:
click "select request file" button, then choose the XMLFile1.xml in ../reposetory folder, click open
click "add dll files" button, then choose the tc1.dll tc2.dll td1.dll and td2.dll in ../reposetory folder, click open
click send test request;

when ask result:
modify the text box after datetime, then click send query request

requirement3:
please dont choose any dll file(at least dont choose all) and send the test request again

Folders:
./logs is the folder that the client receive all the log files(.txt)
./repo is the repository that receive all the files from client(.dll) and(.xml) and the logs from test harness(.txt), please notice all the file name is modified into [Author]_[Datetime]_[original_name]
./THtemp is the test harness that receive all the files from repository(.dll)(.xml) and the test log created(.txt) by itself 

the end point we use:

test harness string listener: http://localhost:8080/THStrRcvr
file listener:http://localhost:8080/THFRcvr
repository string listener: http://localhost:8081/RepoStrRcvr
file listener:http://localhost:8081/RepoFRcvr
for client, there is a string "url" in the Message, and the string listener should be url+"CStrRcvr"
the file receiver should be url+"CFRcvr"


command table:
Command				from		to			describtion
====================================================================
Test				clent		TH			ask TH to run a test
RunTest				repo		TH			ask TH to execute a test
FileNotMatch		repo		TH			tell TH the file it required cannot found
FileNotMatch		TH			client		tell client the request cannot been done because file not found
Request File		TH			repo		ask repo to send a list of requested file
Request Query		client		repo		ask repo to send the result of the test
NoResult			repo		client		tell client the query request cannot been done because we dont have the result about that test
