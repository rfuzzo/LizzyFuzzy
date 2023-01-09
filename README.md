
![GitHub Workflow Status](https://img.shields.io/github/actions/workflow/status/rfuzzo/LizzyFuzzy/check.yml)
<a href="https://discord.gg/cp77modding"><img src="https://img.shields.io/discord/717692382849663036.svg?label=&logo=discord&logoColor=ffffff&color=7389D8&labelColor=6A7EC2"></a>

# LizzyFuzzy

A simple discord bot for the Cyberpunk Modding Community discord server written in c# using [https://github.com/discord-net/Discord.Net](https://github.com/discord-net/Discord.Net).

## Slash Commands

### Modding

| Command | Description | Requirements |
| --- | --- | --- |
| `gh-issue` | Create a new GitHub Issue in WolvenKit | RequireRole |
| `registermod` | Register a nexus mod with the bot | |
| `mod` | Get the Nexus link to a registered mod | |
| `deletemod` | Delete a registered mod | GuildPermission.ManageMessages |
| `wiki` | Get editing info for the modding community wiki | |
| `info` | Send info on the selected modding tool | |

### Common

| Command | Description | Requirements |
| --- | --- | --- |
| `poll` | Start a poll | |
| `ping` | Check the bot connection | |
| `meme` | Generate a meme from a memegen template | |

## Message Commands

| Command | Description | Requirements |
| --- | --- | --- |
| Check DDS Format | Right click on an uploaded .dds file to get the DXGI format | |
| Get custom emote url | Right click on a single emote message to get the emote url | |

## User Commands

| Command | Description | Requirements |
| --- | --- | --- |
| Get user avatar | Right click on a user to get the avatar url | |
