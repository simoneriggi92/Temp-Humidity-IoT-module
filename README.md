# Temperature and Humidity IoT Module

**[GitHub Repository](https://github.com/simoneriggi92/Simone_portfolio)**

An IoT module designed for a rural environment, capable of detecting **humidity, lighting, and temperature** through three different sensors. These measurements are read and handled by a **FEZ Spider II board**.

## Project Overview

The **FEZ microcontroller** collects data from connected sensors and stores it on an **SD card**. These data are transmitted using the **HTTP protocol** in **JSON format** to an **ASP.NET Web Service API**, which stores them in a **Microsoft SQL Server** database.

Using the **MQTT protocol**, the Web Service publishes data to two different topics:
- To configure **AWS**
- To send measurement data

A **Python AWS Lambda function** deserializes the received data and stores it in **DynamoDB** tables.

![image](https://github.com/user-attachments/assets/bfe42235-abdd-4115-ada7-e402747d876e)

## System Components

### Sensor
![image](https://github.com/user-attachments/assets/af9857fe-153a-414b-a5b7-82f69e570c69)


The **FEZ Spider II** microcontroller connects to sensors for:
- Humidity  
- Light  
- Temperature

These sensors capture environmental conditions and send readings to the microcontroller.

### ACK/NACK Mechanism

![image](https://github.com/user-attachments/assets/67173791-a915-40e9-b4cd-7e12097d12b2)


A **Python Lambda function** sends **ACK** or **NACK** messages back to the Web Service:
- **ACK**: Confirms successful storage; the Web Service informs the microcontroller to delete the data from the SD card.
- **NACK**: Indicates failure; the microcontroller resends the data.

  ![image](https://github.com/user-attachments/assets/cc273064-9271-47e7-960b-9941db006794)


### Interface

A **Web App** built using **Node-RED** visualizes the stored data in real-time charts via **AWS API** and **DynamoDB**.

![image](https://github.com/user-attachments/assets/da70568d-1414-4e1e-83b6-c0d1d8fb5ab4)

## Technical Workflow

- **Microcontroller logic** developed in **C#**:
  - Reads sensor data.
  - Stores it on an SD card.
  - Maintains an internal log of operations.
  - Waits for an ACK from the Web Service before deleting stored data.

- **ASP.NET Web Service**:
  - Exposes APIs to receive/send microcontroller data.
  - Stores data and ACK/NACK statuses in **Microsoft SQL Server**.
  - Acts as an **MQTT broker**, forwarding data to **AWS**.

- **AWS Integration**:
  - **Lambda Function**: Deserializes JSON payloads and stores data in **DynamoDB**.
  - Sends ACK or NACK based on data processing outcome.

- **Frontend**:
  - Developed in **Node-RED**.
  - Fetches data using **DynamoDB API**.
  - Displays real-time charts for monitoring.

---

This project simulates a complete IoT data acquisition and processing pipeline—from edge sensor readings to cloud storage and visualization—with robust feedback mechanisms for data integrity.

