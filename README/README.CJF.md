<h1 align="center">Config.json File</h1>

- [README](https://github.com/Yti890/Discord-Module/blob/master/README.md)
- [LabAPI README](./README.LabAPI.md)
- [Create Discord Bot README](./README.CDB.md)
- [Localization bot logs. README](./README.Localization.md)

<h2 align="center">How configure the execution of game commands through Discord</h2>

- Open your bot `config.json` file.
- Add role IDs and list every command they can execute. You can use `.*` to permit to that role ID to use all game commands without restrictions.
  
```json
  "ValidCommands": {
    "1": {
      "953784342595915779": [
        "di"
      ]
    }
  },
 ```

- **Never duplicate commands.** Higher roles on your Discord server will be able to use lower roles commands as well, based on the position of the roles.

<h2>Available commands</h2>

| Command | Description | Arguments | Permission | Example |
| --- | --- | --- | --- | --- |
| di playerlist | Gets the list of players in the server. | | di.playerlist | **di playerlist** |
| di stafflist | Gets the list of staffers in the server. | | di.stafflist | **di stafflist** |

<h2 align="center">How configure the Automatic Roles through Discord</h2>

- Open your bot `config.json` file.
- Add role IDs and "your server group name". If you gave owner and admin roles on your Discord server this will be use highter role.

```json
   "DiscordAutomaticRoles": {
    "YourRoleDiscordID": "owner",
    "YourRoleDiscordID": "admin",
    "YourRoleDiscordID": "mod"
  },
 ```
- **Never duplicate commands.** Higher roles on your Discord server will be able to use lower roles commands as well, based on the position of the roles.

- The next step write on your discord server `/sync yoursteamID64@steam` if all good you see `⚠️ Sync complete, but no matching roles were found` ( you no have roles to that ) or `✅ Sync complete. Assigned role: your role`.
- Remebmer if player yoused sync they NOT NEED USE that again.
- And I'll say this too, if you take his role away, it will automatically be removed in the Game, if you give it back it will have the same effect, even if he leaves the Discord server, it will have the same effect.
