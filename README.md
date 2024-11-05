# SMSGatekeeper

How to start: 

1. Download and build solution
2. You are supposed to have an RabbitMQ installed in you PC (as service or docker conatiner)
   
<img width="752" alt="image" src="https://github.com/user-attachments/assets/217cb545-c2eb-487e-ad1a-bb068c4e6632">

3. Run SMSProcessor.exe
4. Be sure proper queue has been registered in RabbitMQ

<img width="593" alt="image" src="https://github.com/user-attachments/assets/ad7e0c8d-0340-4c54-b8c4-4eadf705cb71">

   
5. Run Fake.Senders.exe. You will be proposed to type number of messages you'd like to send.
   
   <img width="577" alt="image" src="https://github.com/user-attachments/assets/5384dc05-ea9f-4562-ad18-f3752f83476c">
   
6. Then navigate back to the SMSProcessor console. It should looks like this
   
   <img width="867" alt="image" src="https://github.com/user-attachments/assets/8e139f84-a960-4a7d-8b2e-2286acb55b1c">
   
7. If there are now available slots for sending you should be experienced with this stuff

   <img width="505" alt="image" src="https://github.com/user-attachments/assets/c0830daf-15a3-4b0e-bbab-5d5dd9bacb52">

   And small statistic file appers in Files folder (I know it looks a bit uncompleted - sorry about that :))

   <img width="343" alt="image" src="https://github.com/user-attachments/assets/e3b2e70d-b149-4e2f-8894-7d90568653e5">

   3 + 3 + 3 + 2 + 2 == 13 and we have limit in our appsettings file

   <img width="413" alt="image" src="https://github.com/user-attachments/assets/1582fa64-626d-42f3-90d6-3838794bc1ab">



How it works:

   SMSQueueConsumerService is an background self hosted service where we register all our consumers for RabbitMQ queue.
The heart of the system is an dispather(SMsDispatcher) where we inspect, count and manage all telephone numbers available for sending.
The dispatcher calls an factory to crete worker to send SMS. When worker is done with his job it receives callback and tells to the dispatcher:
"Please release this number - it is avalible for use now".

 <img width="541" alt="image" src="https://github.com/user-attachments/assets/3ad80277-e3d5-416d-9fb8-28ae7350a8dc">


 How to scale it:

 1. Approach number one - we can increase the number of consumers and add some queues in RabbitMQ
 2. Approach number two - cover our servicen in Docker conatiner and depends on performance we could start new one. ATTENTION! If follow this approach it will be necessary to move dispatcher in dedicated conatiner to share data with several services containers. It should look like this

    <img width="349" alt="image" src="https://github.com/user-attachments/assets/c34c24fb-5d1e-48de-a63e-d5a5f553e684">


What still in progress:

1. Start service in conatiner - not sure will be ready till tomorrow noon
2. Web interface for statistic (bonus project) - not sure will be ready till tomorrow noon
3. Cover all this stuff with tests... 
