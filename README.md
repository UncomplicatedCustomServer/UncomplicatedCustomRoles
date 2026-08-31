<div align="center"><a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/releases/latest"><img src="https://img.shields.io/github/v/release/UncomplicatedCustomServer/UncomplicatedCustomRoles"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/releases/latest"><img src="https://img.shields.io/github/downloads/UncomplicatedCustomServer/UncomplicatedCustomRoles/total"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/pulls"><img src="https://img.shields.io/github/issues-pr/UncomplicatedCustomServer/UncomplicatedCustomRoles"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/pulls"><img src="https://img.shields.io/github/issues-pr-closed/UncomplicatedCustomServer/UncomplicatedCustomRoles"></a> <a href="https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/commits/main/"><img src="https://badgen.net/github/commits/UncomplicatedCustomServer/UncomplicatedCustomRoles/main"></a> <a href="https://opencollective.com/ucs"><img src="https://img.shields.io/opencollective/all/ucs?label=OpenCollective%20backers&color=7FADF2"></a>

  <img src="https://raw.githubusercontent.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/refs/heads/resources/ucr_promo_banner.png">
  <i>Easy, fully configurable and customizable custom roles for your SCP:SL Server!</i>

  <br><br>
  <br><br>
    <a href='https://discord.gg/5StRGu8EJV'><img src='https://www.allkpop.com/upload/2021/01/content/262046/1611711962-discord-button.png' height="100"></a>
  <br><br>
</div>

## Requirements
- **LabAPI** >= `v1.x`
- **Harmony** (`0Harmony.dll`) >= `v2.x`

<br>

## Localized READMEs
- [&#127467;&#127479; Français](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-FR.md)
- [&#x1F1EE;&#x1F1F9; Italiano](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-IT.md)
- [&#127479;&#127482; Русский](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-RU.md)
- [&#127465;&#127466; Deutsch](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-DE.md)
- [&#127477;&#127473; Polski](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-PL.md)
- [🇨🇳 简体中文](https://github.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/blob/main/Localization/README-CN.md)

> [!NOTE]
> The localized READMEs are written and updated by volunteers from the community, so they can fall behind this one.
> If a translation is missing something, or says something different, **this English README is the one that is up to date**.

## What's UncomplicatedCustomRoles
**UncomplicatedCustomRoles** or **UCR** is a plugin for **LabAPI** that lets you create fully configurable and customizable custom roles with YAML.\
A custom role starts from a normal SCP:SL role and changes whatever you want about it: health, items, effects, spawn point, team, custom info and much more.\
With UCR, you can fully customize your Custom Roles by modifying almost every setting, allowing you to create whatever you can imagine: the only limit is your imagination

## Features
### 🖥️ Fully customizable Custom Roles
Health, AHP, hume shield, stamina, scale, nickname, badge, role name, custom info, effects, damage multiplier, inventory, ammo, item limits and custom items: make your SCP:SL server one-of-a-kind!
### 🎯 Spawns exactly where and when you want
Pick how every role spawns: a random room, a whole zone, specific rooms, a spawn point you saved in-game, the position of another role, the Class-D cells, or simply wherever the player already is.\
Then tune when it happens: spawn chance, spawn delay, how many can be alive at the same time, the minimum amount of players, which vanilla roles it may replace and the permission a player needs to get it.
### 🧩 More than 25 built-in modules
Add extra mechanics to a role with a single line of config: damage resistance, life stealing, custom keycard permissions, item bans, pacifism until the role takes damage, silent footsteps, tesla gates and SCP-096 that ignore the role, a fake team, a custom custom info layout, a schematic attached to the player, and many more.
### 🚪 Escape system
Decide whether a role can escape and which role it becomes afterwards — with a different outcome depending on whether it escaped free or cuffed, and by whom — and let it keep its inventory on the way out.
### 🔗 Plays well with your other plugins
UCR integrates out of the box with **UncomplicatedCustomItems**, **UncomplicatedCustomTeams**, **Exiled CustomItems**, **ScriptedEvents**, **SLWardrobe**, **RespawnTimer** and **LabApiExtensions**.
### 🧪 Built-in role validator
Every role is checked while it is being loaded, and UCR tells you in the console what is wrong with it, so a typo in a config does not turn into a broken round.
### ⌨️ In-game commands
Manage your custom roles using the many built-in commands provided by UCR:\
`ucr list`, `ucr info`, `ucr role`, `ucr spawn`, `ucr cinfo`, `ucr reload`, `ucr spawnpoint`, `ucr percentages`, `ucr errors`, `ucr generate`, `ucr update`, `ucr version` and more.
### 🗂️ YAML based
You don't need to know how to code to use UCR: custom roles are created using `yml` ([YAML](https://en.wikipedia.org/wiki/YAML)) files, an extremely easy and intuitive serialization language!
### 📑 Exhaustive documentation
Check out the [official UCR documentation](https://docs.ucr.ucserver.it/), where you'll find everything you need to use the plugin: from getting started to advanced settings!
### 🔌 Designed for developers
UCR was also designed to make life easier for developers: integrating with the plugin is simple, intuitive, and well-documented!\
[Check it out!](https://docs.ucr.ucserver.it/developers/intro)
### 🫂 Active community on Discord
Join our Discord community to chat with other users and even the developers!\
Interact, ask for help, share your knowledge and your roles: we’d love to have you!

## Bugs and plan
To track bugs and UCR planning, UCSC has made a FlySpray instance available.\
[bugs.ucserver.it](https://bugs.ucserver.it/index.php?project=2&do=index&switch=1)

## Installation
Check the [documentation](https://docs.ucr.ucserver.it/getting-started/installation)

## If you use UCR, please consider making a donation
UCR is a plugin made by **UCS Collective**.\
Every plugin we create is **free** and **open-source**, and it always will be.\
What there is, is the time we spend writing the plugin, answering your questions on Discord and keeping everything working after every SCP:SL update.\
If UCR is running on your server, **please consider donating something through OpenCollective** — every contribution, however small, goes straight back into the plugins you are using:

<a href="https://opencollective.com/ucs"><img height="15" src="https://raw.githubusercontent.com/UncomplicatedCustomServer/UncomplicatedCustomRoles/refs/heads/resources/oc_icon.png">&nbsp;&nbsp;Donate</a>

## Contacts
### UCS - UncomplicatedCustomServer
  **Discord:** [https://discord.gg/5StRGu8EJV](https://discord.gg/5StRGu8EJV)

### FoxWorn3365
  **Discord:** `@foxworn`\
  **Email:** `foxworn3365@gmail.com`
### MedveMarci
  **Discord:** `medvemarci`
### Dr.Agenda
  **Discord:** `dr.agenda`

## Translation Credits
**Français:** `@robocnop`\
**Italiano:** `@foxworn`\
**Русский:** `@naxefir`\
**Deutsch:** `@seekedstroy`\
**Polski:** `@.piwnica2137`\
**简体中文:** `@Raiden-Yayi`