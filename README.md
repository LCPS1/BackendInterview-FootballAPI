# Clean API with CRUD Operations and Background Services

## Overview

This repository implements a CRUD API using clean code principles, design patterns, and a focus on maintainability, scalability, and testability. The project showcases the usage of design patterns like the **Factory Pattern**, **Repository Pattern**, and **Unit of Work** to properly structure the code.

Additionally, the project uses **OOP** principles for better organization and traceability. In the future, the code could be migrated to a more structured solution, such as moving to Azure Functions or other cloud-based solutions.

---

## Features

- **CRUD Operations**: Basic Create, Read, Update, and Delete operations.
- **Background Services**: Handles long-running tasks independently from user requests.
- **Design Patterns**: Implements Factory, Repository, and Unit of Work patterns to keep the application maintainable and scalable.
- **Clean Code**: Emphasis on writing clean, readable, and maintainable code.

---

## Setup and Installation

This project is designed to be run with Docker. Follow these steps to get it up and running:

1. **Clone the repository**

   ```bash
   git clone https://github.com/LCPS1/BackendInterview.git
   cd BackendInterview

2. **Build and run the application using Docker**

   Ensure you have Docker installed, then run the following command to build and start the containers:

   ```bash
   docker-compose up --build -d
   
3. **Access the API**

   Once the containers are running, open your browser and visit:

   ```bash
   http://localhost:8080/swagger/index.html
