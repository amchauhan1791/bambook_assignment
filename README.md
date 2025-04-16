# 🧩 Discount.CustomDiscounts Plugin for NopCommerce 4.80

This plugin adds custom discounts of 10% (% is configurable) to your regular customers, including new views, localized resources. It is designed for NopCommerce version **4.80.1** and built on **.NET 9**.

---

## 📦 Installation

Follow the steps below to install the plugin into your NopCommerce solution:

1. **Clone or Download the Plugin**
   ```bash
   git clone https://github.com/amchauhan1791/bambook_assignment.git
   ```

2. **Move the Plugin Folder**
   - Copy the entire `Discount.CustomDiscounts` folder into:
     ```
     /Plugins/
     ```

3. **Update the Solution**
   - Add the `.csproj` file to your `NopCommerce.sln` solution file.
   - Right-click on the `Plugins` folder in Solution Explorer → **Add → Existing Project**.

4. **Build the Plugin**
   - Ensure your output path in `Discount.CustomDiscounts.csproj` points to:
     ```
     $(SolutionDir)\Presentation\Nop.Web\Plugins\Discount.CustomDiscounts
     ```
   - Build the plugin project. This will copy required DLLs, views, and resources to the correct plugin output folder.

5. **Enable the Plugin**
   - Run your NopCommerce site.
   - Navigate to **Admin → Configuration → Plugins → Local plugins**.
   - Find `Discount.CustomDiscounts` and click **Install**.

---

## 🔌 Testing the Order details API Endpoint

This custom API endpoint. You can test it using **Postman**, **cURL**, or any HTTP client.

### Example Request
### Generate Token
1. First, you have to authenticate the user; for that, you have to call the GenerateToken API endpoint as below

```http
GET http://localhost:15536/api/orderApi/GenerateToken
```
### Expected Request
```Headers
{
  "content-type": "application/json" 
}

```json body
{
  "userName": "admin",
  "password": "admin123"
}

### Expected Response

```json
{
    "Token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiJhZG1pbiIsImp0aSI6IjFiMjhjYWE1LWQ3ZmEtNDI0Ni05NWRkLWE4ODU5NThmMzY3MCIsImV4cCI6MTc0NDc5Nzk4NiwiaXNzIjoiQmFtYm9vIENhcmRzIiwiYXVkIjoiQ3VzdG9tZXJzIG9yIFN0YWZmIn0.6xfhU7-qEDi3h2J4A4sO049ymkFVbcmxztmZhTqVZFY"
}
```
![image](https://github.com/user-attachments/assets/ebbb8256-bbd7-4728-9d15-b25786ae15ce)


> ℹ️ Copy the above token and put as "Bearer #generated_token#" in your sunsequent API request's Authentication parameter

### GetOrderDetails

```http
GET http://localhost:15536/api/orderApi/GenerateToken
```
### Authorization
1. Select #Bearer Token# from the Auth Type dropdown
2. Enter the above token inside the Token field right side
   
![image](https://github.com/user-attachments/assets/e6e420e4-f845-4269-8788-af466dfd0b9a)

### Expected Response

```json
{
    "OrderId": 2,
    "TotalAmount": 2460.0000,
    "OrderDate": "4/11/2025"
}
```
---

## 🐳 Building & Running a Containerized Version

If you want to run the full NopCommerce app inside the Docker, execute the following command:

### 1. **Build the Docker Image**

> You should already have a Dockerfile in your NopCommerce root directory.

```bash
 docker compose build --no-cache
```

### 3. **Run the Container**

```bash
 docker compose up -d
```

Visit: [http://localhost:8080](http://localhost:8080)

---

## ☁️ Azure Deployment

To deploy your NopCommerce app (with this plugin) to **Azure App Service**:

### 1. **Publish Using Visual Studio**
- Right-click on `Nop.Web` → **Publish**
- Choose **Azure App Service (Windows/Linux)** or **Azure Container Registry**

### 2. **App Service Configuration**
- Make sure the plugin folder is deployed to:
  ```
  /wwwroot/Plugins/Discount.CustomDiscounts
  ```

- Set environment variable (if using containers):
  ```env
  ASPNETCORE_ENVIRONMENT=Production
  ```

### 3. **Install Plugin After Deployment**
- Once deployed, go to:
  ```
  https://your-azure-site/admin
  ```
- Navigate to **Configuration → Plugins → Local plugins**
- Install and enable the plugin from there

---

Made with ❤️ by **Ajay Chauhan**

