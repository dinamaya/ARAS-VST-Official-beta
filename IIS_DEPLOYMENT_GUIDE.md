# IIS Deployment Guide for AR Adjustment System

This guide outlines the steps to deploy the 5 components of the AR Adjustment System to an internal UAT IIS Server under the single domain: `https://uat-aradjustment.vstecs.com.ph`.

## Components Overview
1. **ARAS.Blazor** (Frontend UI) - Hosted at the Root (`/`)
2. **ARAS.Gateway** (Ocelot API Gateway) - Hosted as an IIS Application (e.g., `/gateway`)
3. **ARAS.Main.SSMS.Api** (Backend API) - Hosted as an IIS Application (e.g., `/ssms-api`)
4. **ARAS.Main.Oracle.Api** (Backend API) - Hosted as an IIS Application (e.g., `/oracle-api`)
5. **ARAS.OracleSync.Worker** (Background Task) - Hosted as an IIS Application (e.g., `/worker`)

---

## 1. Prerequisites on the UAT Server

Before copying any files, ensure the target Windows Server running IIS has the necessary components installed:
1. **.NET 9 Hosting Bundle**: Download and install the [ASP.NET Core Runtime 9.0 Hosting Bundle](https://dotnet.microsoft.com/download/dotnet/9.0). This installs the `.NET Core Windows Server Hosting` module for IIS.
   - *Restart IIS after installation*: Open Command Prompt as Administrator and run `iisreset`.
2. **WebSockets Protocol**: The `ARAS.Blazor` app (Blazor Server) requires WebSockets for SignalR communication.
   - Open **Server Manager** > **Add Roles and Features**.
   - Navigate to **Server Roles** > **Web Server (IIS)** > **Web Server** > **Application Development**.
   - Ensure **WebSocket Protocol** is checked and installed.
3. **Application Initialization**: The `ARAS.OracleSync.Worker` needs to run continuously in the background.
   - In **Add Roles and Features** > **Server Roles** > **Web Server (IIS)** > **Web Server** > **Application Development**.
   - Ensure **Application Initialization** is checked and installed.

---

## 2. Directory Structure Setup

Create a dedicated folder structure on the server to hold your published artifacts. For example, under `C:\inetpub\wwwroot\ARAS`:

```text
C:\inetpub\wwwroot\ARAS\
    ├── Blazor\      (Copy contents of ARAS.Blazor publish here)
    ├── Gateway\     (Copy contents of ARAS.Gateway publish here)
    ├── SSMS\        (Copy contents of ARAS.Main.SSMS.Api publish here)
    ├── Oracle\      (Copy contents of ARAS.Main.Oracle.Api publish here)
    └── Worker\      (Copy contents of ARAS.OracleSync.Worker publish here)
```

**File Permissions**: Ensure the `IIS_IUSRS` group has **Read & Execute** permissions on the `C:\inetpub\wwwroot\ARAS` folder and all its subfolders.

---

## 3. Configuring IIS Application Pools

For .NET Core/.NET 9 applications, it is best practice to create separate Application Pools for each application and set them to **No Managed Code**.

1. Open **Internet Information Services (IIS) Manager**.
2. Right-click **Application Pools** > **Add Application Pool...**.
3. Create the following 5 App Pools with **.NET CLR version** set to `No Managed Code` and **Managed pipeline mode** set to `Integrated`:
   - `ARAS_Blazor_AppPool`
   - `ARAS_Gateway_AppPool`
   - `ARAS_SSMS_AppPool`
   - `ARAS_Oracle_AppPool`
   - `ARAS_Worker_AppPool`

### Special Configuration for the Background Worker App Pool
Because the `ARAS.OracleSync.Worker` is a background worker hosted in IIS, IIS will naturally try to spin it down when there is no incoming HTTP traffic. You must configure its App Pool to stay alive:
1. Select `ARAS_Worker_AppPool`, right-click > **Advanced Settings**.
2. Set **Start Mode** to `AlwaysRunning`.
3. Set **Idle Time-out (minutes)** to `0`.
4. (Optional but recommended) Do the same for the other API App Pools if you want to avoid cold start delays.

---

## 4. Setting up the Main IIS Site (Blazor Frontend)

The root domain (`https://uat-aradjustment.vstecs.com.ph`) will point directly to the Blazor application.

1. In IIS Manager, right-click **Sites** > **Add Website...**.
2. **Site name**: `ARAS_UAT`
3. **Application pool**: Select `ARAS_Blazor_AppPool`.
4. **Physical path**: `C:\inetpub\wwwroot\ARAS\Blazor`
5. **Binding**:
   - Type: `https`
   - Host name: `uat-aradjustment.vstecs.com.ph`
   - Select your valid SSL certificate.
6. Click **OK**.

---

## 5. Setting up the Sub-Applications (Gateway, APIs, Worker)

Now add the remaining 4 projects as Sub-Applications under the main `ARAS_UAT` site.

1. Expand the `ARAS_UAT` site in the left pane.
2. Right-click the `ARAS_UAT` site > **Add Application...**.
3. **Gateway**:
   - **Alias**: `gateway` (This makes the URL `https://uat-aradjustment.vstecs.com.ph/gateway`)
   - **Application pool**: `ARAS_Gateway_AppPool`
   - **Physical path**: `C:\inetpub\wwwroot\ARAS\Gateway`
4. **SSMS API**:
   - **Alias**: `ssms-api`
   - **Application pool**: `ARAS_SSMS_AppPool`
   - **Physical path**: `C:\inetpub\wwwroot\ARAS\SSMS`
5. **Oracle API**:
   - **Alias**: `oracle-api`
   - **Application pool**: `ARAS_Oracle_AppPool`
   - **Physical path**: `C:\inetpub\wwwroot\ARAS\Oracle`
6. **Worker**:
   - **Alias**: `worker`
   - **Application pool**: `ARAS_Worker_AppPool`
   - **Physical path**: `C:\inetpub\wwwroot\ARAS\Worker`

### Special Configuration for the Background Worker Application
To ensure the worker auto-starts when IIS restarts:
1. Select the `worker` sub-application you just created.
2. In the right-hand **Actions** pane, click **Advanced Settings**.
3. Set **Preload Enabled** to `True`.

---

## 6. Configuration Updates for the Environment

Since you are hosting these APIs in sub-paths (e.g., `/gateway`), you must manually update your application configuration files (`appsettings.json`, `ocelot.json`, etc.) on the UAT server after publishing.

### A. Updating Ocelot (`ocelot.json` in Gateway)
Update the `DownstreamHostAndPorts` and `GlobalConfiguration.BaseUrl` to point to the new IIS setup.

Since the APIs are hosted on the same server, Ocelot can route to them via `localhost` but must include the new IIS aliases in the path, or use the public domain. For example:

```json
{
  "Routes": [
    {
      "DownstreamPathTemplate": "/ssms-api/api/adjustments/{everything}",
      "DownstreamScheme": "https",
      "DownstreamHostAndPorts": [
        {
          "Host": "localhost",
          "Port": 443
        }
      ],
      "UpstreamPathTemplate": "/adjust/{everything}",
      "UpstreamHttpMethod": [ "GET", "POST" ]
    }
  ],
  "GlobalConfiguration": {
    "BaseUrl": "https://uat-aradjustment.vstecs.com.ph/gateway"
  }
}
```
*Note: Depending on how your APIs are structured, you may need to adjust the `DownstreamPathTemplate` to match the exact route the API expects when hosted as a sub-application in IIS.*

### B. Updating the Blazor App
Ensure the Blazor app's `appsettings.json` points to the Gateway's new URL for making API calls:
```json
{
  "GatewayBaseUrl": "https://uat-aradjustment.vstecs.com.ph/gateway"
}
```

Ensure your `wwwroot/index.html` or `App.razor` `<base href="/" />` tag is correct. Since Blazor is at the root domain, `<base href="/" />` is correct.

---

## 7. Troubleshooting Tips
- **HTTP 500.19 Errors**: Usually means the .NET Core Hosting Bundle is missing or IIS_IUSRS lacks read permissions to the web.config or publish folder.
- **SignalR / WebSocket Issues in Blazor**: Ensure the **WebSocket Protocol** is installed in IIS Server Roles.
- **Worker App Not Processing Background Tasks**: Ensure **AlwaysRunning** (App Pool) and **Preload Enabled** (Application) are set.
- **Routing Issues (404s) from Gateway**: When an ASP.NET Core app is hosted as an IIS Sub-Application (e.g., `/ssms-api`), the `PathBase` is automatically set to `/ssms-api`. Ensure your Ocelot routes correctly account for this.
