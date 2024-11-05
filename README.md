# SMSGatekeeper

How to start: 

1. Donload and build solution
2. You are supposed to have an RabbitMQ installed in you PC (as service or docker conatiner)
   
<img width="752" alt="image" src="https://github.com/user-attachments/assets/217cb545-c2eb-487e-ad1a-bb068c4e6632">

4. Run SMSProcessor.exe
5. Run Fake.Senders.exe. You will be proposed to type number of messages you'd like to send.
   
   <img width="577" alt="image" src="https://github.com/user-attachments/assets/5384dc05-ea9f-4562-ad18-f3752f83476c">
   
7. Then navigate back to the SMSProcessor console. It should looks like this
   
   <img width="867" alt="image" src="https://github.com/user-attachments/assets/8e139f84-a960-4a7d-8b2e-2286acb55b1c">


How it works:

SMSQueueConsumerService is an background self hosted service where we register all our consumers for RabbitMQ queue.
The heart of the system is an dispather(SMsDispatcher) where we inspect, count and manage all telephone numbers available for sending.
The dispatcher calls an factory to crete worker to send SMS. When worker is done with his job it receives callback and tells to the dispatcher:
"Please release this number - it is avalible for use now".

 <img width="541" alt="image" src="https://github.com/user-attachments/assets/3ad80277-e3d5-416d-9fb8-28ae7350a8dc">
