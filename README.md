# ASP.NET Core Telegram Bot 

## About 

This Telegram bot works with the VLSU API and provides information about the class schedules of groups and teachers.
This project works with the [Telegram.Bot](https://github.com/TelegramBots/Telegram.Bot) library

## Preview
### Finding a schedule for a group

https://github.com/user-attachments/assets/c10c02a3-fc21-43fc-a357-b5622ef34f69

### Finding a schedule for a teacher

https://github.com/user-attachments/assets/dcf0edc5-0fc0-4389-b4aa-e37b79fc1d6b

## Setup

Please make sure you have .NET 8 or newer installed. You can download .NET runtime from the [official site.](https://dotnet.microsoft.com/download)

## Configuration

You have to specify your Bot token in **VLSU.ScheduleTelegramBot.API/appsettings.json**. 

Replace `BotToken` in **appsettings.json** with actual Bot token. 

Also you have to specify endpoint, to which Telegram will send new updates with `BotWebhookUrl` parameter.

To make sure that the webhook was configured by us, you need to specify the secret data in the `SecretToken` parameter.
The request from Telegram will contain the header "X-Telegram-Bot-Api-Secret-Token" with the secret token as the content, which will be processed in the **BotController**.
More: [WebHook](https://core.telegram.org/bots/api#setwebhook)

```json
  "BotOptions": {
    "BotToken": "<Token from BotFather>",
    "BotWebhookUrl": "<Https url from Ngrok (Clo) or your VPS>",
    "SecretToken": "<1-256 characters. Only characters A-Z, a-z, 0-9, '_' and '-' are allowed.>"
  },

  "ConnectionStrings": {
    "MSSql": "<Connection string to MS Sql Server>"
  }
```

## Ngrok

Ngrok gives you the opportunity to access your local machine from a temporary subdomain provided by ngrok. This domain can later send to the telegram API as URL for the webhook.
Install ngrok from this page: [ngrok - download](https://ngrok.com/download) or via homebrew cask:

```shell
brew install --cask ngrok
```

and start ngrok on port 8443.

```shell
./ngrok http 8443 
```

Telegram API only supports the ports 443, 80, 88 or 8443. Feel free to change the port in the config of the project.

## Clo

```shell
./clo publish http 8443
```

## Run Bot

You need to update the database with the command in the **Package Manager Сonsole**
```shell
Update-Database
```

Now you can start the Bot in a local instance. Check if the port of the application matches the port on which Ngrok (Clo) is running.

