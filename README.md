## Contents
- [Project description](#aimidge)
- [Project main functions](#main-functions)
- [Used technologies](#used-technologies)
- [Architecture](#architecture)
- [Instructions](#instructions-on-how-to-use-the-project)
- [Authors](#authors)




# AIMIDGE

The project idea is a webpage designed for users to generate unique images using artificial intelligence technology. Users can enter text into a specified field, and the system uses a trained artificial intelligence algorithm to create an image corresponding to the input text. The webpage allows users to generate images from English or Lithuanian text input, download those images, and choose their resolution. Additionally, registered users will have the option to save those images to personal albums, which they can download later on. Unregistered users will also be able to generate images, but they will not be able to save them to personal albums. Unregistered users will only be able to generate 3 images during one session.



## Main functions
- Image generation based on input text using trained artificial intelligence algorithm.
- English or Lithuanian input.
- Image editing (changing resolution).
- Option for registered users to save to albums generated images.
- Ability to download generated images.
- Limit the number of images unregistered users can generate per session.

## Used technologies
- Project management tool: Jira
- Retrospective: Parabol
- Communication tool: Discord
- Programs used to implement the project: Visual Studio Code, SQL Express
- Code storage tool: Github

## Architecture
![image](https://github.com/FluffyDump/Aimidge/assets/113236180/1c90f1c0-29f8-4a5b-8741-0a49a9f9d05d)

## Instructions on how to use the project
<strong>1.</strong> Users are welcomed on the introduction page (Figure 1), on which they can "Continue as Guest" or "Login". At the top right corner users can change the language of the website (Figure 2).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/63329cc8-050e-4830-8f2d-e4cc4a25ce8a"></p>
<p align="center"><strong>Figure 1. Introduction page</strong></p>

<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/19f7921d-0dae-40a5-88f7-645189123139"></p>
<p align="center"><strong>Figure 2. Option to change website language</strong></p>

If "Continue as Guest" is pressed users will be redirected to the generator page, if chosen "Login" they will be redirected to the login page (Figure 3).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/263f247c-8405-406f-8be2-096979561ff4"></p>
<p align="center"><strong>Figure 3. Login page</strong></p>

On the login page, if the user has previously registered he can log in with his credentials, if not then when the button "Sign up" is pressed users can register with their credentials (Figure 4), and they will be redirected to the generator page.
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/2228a1a9-21ff-416e-b306-e9039d29cdd2"></p>
<p align="center"><strong>Figure 4. Sign up page</strong></p>

<strong>2.</strong> When users are redirected to the generator page (Figure 5), they can enter English or Lithuanian prompts into a text box and choose what resolution they want their image to be. When pressed "Submit" their image will be generated (Figure 6).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/30a7c541-9c46-42d7-98e8-700399fa4cba"></p>
<p align="center"><strong>Figure 5. Generator page</strong></p>

<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/2599745f-8a85-44a4-912a-ea0efe0022c2"></p>
<p align="center"><strong>Figure 6. Generated image</strong></p>


After generation users will have two options: to download the image or to clear history. Additionally, registered users will have an option to save the generated image to their album.

<strong>3.</strong> On the left side of the screen there is a sidebar (Figure 7).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/fa7dd184-7c52-4d0a-9d8f-d7234f8e7eba"></p>
<p align="center"><strong>Figure 7. Generated page with opened sidebar</strong></p>


  After expanding it, users will have these options: 
- Go back to the introduction  page, when pressed on "AIMIDGE" or the site's logo (Figure 8).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/be287b3e-9803-4b04-9321-fd817a1d361f"></p>
<p align="center"><strong>Figure 8. Button to go back to introduction page</strong></p>

- Go to the generator page (Figure 9).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/3deaaf44-5c6f-4d21-8996-dea492ffbb8a"></p>
<p align="center"><strong>Figure 9. Button to go to generator page</strong></p>

Additionally, registered users will have these options:

- Go to the album page (Figure 10). 
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/f1f4c087-e6cc-475c-8805-ab5641c5886d"></p>
<p align="center"><strong>Figure 10. Button to go to album page</strong></p>

- Go to the user profile or logout (Figure 11).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/663d36d1-4e6a-4fc5-82fc-03d11630083c"></p>
<p align="center"><strong>Figure 11. Buttons to go to user profile page or logout</strong></p>

<strong>4.</strong> When the "Album" button is pressed, users will be redirected to the album page (Figure 12).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/67b02fbd-6675-4b04-a577-2428fd4d4b89"></p>
<p align="center"><strong>Figure 12. Album page</strong></p>

Here users will see their saved generated images. When hovering over the image with a mouse, two buttons will appear (Figure 13) and users will have two options: to delete an image from the album, or download it.
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/211e1bd4-e82f-46a6-9241-982aec788b8e"></p>
<p align="center"><strong>Figure 13. Buttons to download or remove image</strong></p>

<strong>5.</strong> When pressed on the user's name button located at the bottom of a sidebar, users will be redirected to the user profile page (Figure 14).
<p align="center"><img src="https://github.com/FluffyDump/Aimidge/assets/113236180/a463f18e-ceac-4e33-823a-e2a41f0eb4ed"></p>
<p align="center"><strong>Figure 14. User profile page</strong></p>

Here users will have two options: to change their profile picture to any of the 18 available pictures and to change their information (username, email, name).

<strong>6.</strong> When the logout button is pressed, the user will be logged out of the system and redirected to the introduction page.
## Authors

- Tomas Petrauskas ([@FluffyDump](https://github.com/FluffyDump)) - programuotojas, produkto vadovas
- Domas Gladkauskas ([@Pupcikas](https://github.com/Pupcikas)) - programuotojas, dizaineris
- Aurelija Vaitkutė ([@Luckaura](https://github.com/Luckaura)) - programuotoja, Jira
- Tomas Kundrotas ([@Tomkun3](https://github.com/Tomkun3)) - programuotojas, dokumentacija
