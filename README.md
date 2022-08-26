<article id="portfolio" class="wrapper style3">
				<div class="container">
					<header>
						<h2><a href="#">Temperature and Humidity IoT module</a></h2>
						<p>IoT module creation inserted in a particular rural environment, to detect the values concerning <strong>humidity, lighting and temperature</strong>. Measurements obtained from three different sensors and sent to the<strong>FEZ Spider II board</strong>.</p>
					</header>
					<div class="row">
						<div class="col-4 col-6-medium col-12-large">
							<article class="box style2">
								<a href="https://github.com/simoneriggi92/Simone_portfolio" class="image featured"><img src="images/project/send.png" alt="" /></a>
								<p>
									The FEZ microcontroller reads from its sensors and collects data in a <strong>SD card</strong>. The data are then sended though <strong>HTTP</strong> protocol in <strong>JSON</strong> format by <strong>ASP.NET</strong> Web Service API implemented in order to store
									data in a <strong>Miscrosoft SQL</strong> database. Through the <strong>MQTT</strong> protocol, the Web Service publishes data to two different topics in order to set <strong>AWS</strong> configuration and send data.
									The received data are analyzed and deserilized by a <strong>Python lambda function</strong> in order to store data into <strong>DynamoDB</strong> tables.
								</p>
								<div class="row">
									<div class="col-4 col-6-medium col-12-small">
										<article class="box style2">
											<a href="#" class="image featured"><img src="images/project/electronic.png" alt="" /></a>
											<h3><a href="#">Sensor</a></h3>
											<p><strong>The FEZ spider </strong> microcontroller is connected to humidity, light and temperature <strong>sensors</strong>. </p>
										</article>
									</div>
									<div class="col-4 col-6-medium col-12-small">
										<article class="box style2">
											<a href="#" class="image featured"><img src="images/project/ack_nack.png" alt="" /></a>
											<h3><a href="#">Ack and Nack mechanism</a></h3>
											<p>A <strong> Python lambda </strong> function is able to send an <strong>ACK or NACK</strong>message to the WebService in order to delete or re-send the data.</p>
										</article>
									</div>
									<div class="col-4 col-6-medium col-12-small">
										<article class="box style2">
											<a href="#" class="image featured"><img src="images/project/charts.png" alt="" /></a>
											<h3><a href="#">Interface</a></h3>
											<p>A WebApp is able to show stored data by the use of an <strong>AWS API</strong>.</p>
										</article>
									</div>
								<div class="col-4 col-6-medium col-12-large">
									<p>The FEZ microcontroller logic has been developed in C#. It is able to read data from connected sensors and store it in a SD card. It saves all data until an ACK message is
										 received by the Web Service in order to make free space in the SD card. 
										 It contains an internal log in order to register all operations executed by the microcontroller.
										 The ASP.NET Web Service exposes some APIs in order to receive and send data coming from microcontroller. The Miscrosoft SQL Server has been used
										 in order to store measurments data and keep trace of the status (ACKED(NACKED). The Web Service is a Broker as well, since that it
										 must communicates with AWS by the use of MQTT protocol. 

										
										 
										 The data are fowarded by Web Service in a JSON format. The data are deserialized by a Lambda function in order to extract measurments and
										 and store them in a DynamoDB database. 
										 If the data are properly received, an <strong>ACK</strong> message is forwared toward WebService which will forwards too the message to the FEZ spider in 
										 order to delete sended data from its SD card. Otherwise, a <strong>NACK</strong> message will be forwarded and the microcontroller sends the data again. 

										 
										 All stored data are showed in a Web App developed though <strong>Node-READ</strong> through the use of DynamoDB API, in order to show them
										 in related charts to monitoring values in rea-time.
										 
										
								
								
								</div>	

								
							</article>
							
							
						</div>
						
						
						
					</div>
					
				</div>
			</article>

		
