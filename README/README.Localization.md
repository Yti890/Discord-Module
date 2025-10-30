<h1 align="center">Localization</h1>

- [README](https://github.com/Yti890/Discord-Module/blob/master/README.md)
- [Create Discord Bot README](./README.CDB.md)
- [Config.json README](./README.CJF.md)
- [LabAPI README](./README.LabAPI.md)

<h2 align="center">How configure the Language in logs</h2>

- Open your plugins config file.
- Then open `Lang-Configs` and you will see all Languages.
- You might only have en.json, but that's not a problem. It means you have entered the correct folder.

<h2 align="center">How to change the language?</h2>
<p>You can copy the ready-made language packs from the Localization folder here, or create your own.</p>

<h3 align="center">Let's consider creating our own.</h3>

- Copy `en.json` file and paste to any place convenient for you.
- Now, open this file.
- You will see something like...

```json
 {
  "UsedCommand": ":keyboard: {0} ({1}) [{2}] used command: {3} {4}",
  "HasRunClientConsoleCommand": ":keyboard: {0} ({1}) [{2}] has run a client-console command: {3} {4}",
  "NoPlayersOnline": "No players online.",
  "NoStaffOnline": "No staff online.",
  "WaitingForPlayers": ":hourglass: Waiting for players...",
  "RoundStarting": ":arrow_forward: Round starting: {0} players in round.",
  "RoundEnded": ":stop_button: Round ended: {0} - Players online {1}/{2}.",
  "PlayersOnline": "Players online: {0}/{1}",
  "RoundDuration": "Round duration: {0}",
  "AliveHumans": "Alive humans: {0}",
  "AliveScps": "Alive SCPs: {0}",
}
```

<p>How should this be translated correctly?</p>
<p>The answer is simple.</p>

- Look, you have this line: `"UsedCommand": ":keyboard: {0} ({1}) [{2}] used command: {3} {4}",` do not touch `"UsedCommand":, {0} ({1}) [{2}] {3} {4},` the rest of the text like: used command: can be translated.

<p>When you finish, copy this file back to the `Lang-Configs` folder, giving it a name, or copy the text from inside and transfer it to the en.json file in `Lang-Configs`.</p>

<p>I copied the file and renamed it, but my translation didn't change. What should I do?</p>
<p>Open your plugin's config and find the line `code`, it will say `en`, replace it with the name of the file you created, it's important to note that you DO NOT NEED TO WRITE .json.</p>

<h3 align="center">Let's consider the use of language from Localization.</h3>

<p>You can copy the text from the file of the localization you need, or create a new .json file. If you create a .json file and it doesn’t work, follow the instructions above.</p>

- Just copy the text and paste it into the file, that's all.

<p>What should I do if the translation in the Localization file is incorrect or not suitable?</p>
<p>In that case, you can redo it and submit the revised translation in a Pull Request for the specific translation file (which was used) and also explain why the translation is incorrect.</p>
