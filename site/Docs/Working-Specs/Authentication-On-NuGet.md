# Authentication on NuGet.org
The current authentication story on NuGet.org is woefully out of date. Passwords? What is this, 1995? This spec describes a modern authentication model for NuGet. It covers the entire NuGet experience, from Gallery Website to NuGet.exe, from Feed to VS Extension.

## Stories
See the [Auth, Enterprise and Social stories](AuthEnterpriseAndSocialStories) page for detailed stories. Here are some snippets and more focused scenarios.

### Frank creates an account
Frank doesn't really like NuGet, but he's forced to use it. The front page is clear about the fact that login is not required, but also has a nice big "Sign in or Sign up" button. When he clicks it, he's taken to a page with the following buttons:
* Sign in with a Microsoft Account
* Sign in with GitHub
* Sign in with CodePlex
* Sign in with a Username and Password
* Sign in with my Organization Account

**Note:** This UI is already seeming busy...

He has a Microsoft Account, so he decides to click that button. He's asked to allow NuGet.org access to his account, which he does, and is instantly logged in. A welcome message is displayed and it says something about merging accounts if you've already got one, but he doesn't so he ignores it.

### Dora merges accounts
Dora created a NuGet account pretty early, but today when she goes to sign in to the Gallery there are new options for signing in with a Microsoft Account, GitHub account and more. She clicks "Sign in with GitHub" and authorizes NuGet to access her GitHub account. Now she sees a message on NuGet.org indicating that there is already an account using her email address. It tells her to enter the username and password for that account to merge them. She does so and is immediately signed in to her account.

### Frank merges accounts, manually
Frank has a NuGet.org, and he's also noticed that there are new sign-in options. He uses his signs in with a Microsoft Account, but because he uses a different email address on his Microsoft and NuGet accounts, he's immediately logged in as a new user. However, the login page, noticing that this is his first ever log in with the Microsoft Account, contains a button to "Merge with an existing Account". He clicks it and enters his existing user name and password. He is immediately logged in to his old account and a message indicating the merge is displayed.

### Frank merges his GitHub account
Now, Frank wants to link up his GitHub account. He logs out and logs back in with GitHub. Again, on his first login, the message asking if he wants to merge accounts is displayed. Unfortunately, he isn't paying much attention and closes the browser window to go off to lunch. When he returns, he remembers what he was doing and logs back in to NuGet using his GitHub account. Now, he remembers that he was going to merge his account. He clicks his user name in the top right and immediately sees a "Merge with an existing Account" button. He clicks it and continues through the merge flow as before.

### Frank removes his username and password
Frank likes OAuth a lot. He likes it so much, he thinks passwords are way out of date. So, he goes back to his user profile page on NuGet.org and sees a "Manage Login Credentials" option. He clicks it and sees a list (The [] indicate buttons):

* Password: ****** [Change] [Remove]
* GitHub: frank123 [Remove]
* Microsoft Account: frank@outlook.com [Remove]

Frank clicks the "Remove" button next to his Password credential and a confirm window is displayed: "Are you sure you want to remove this credential? You will not be able to log in with it again. NuGet Gallery Support will not be able to retrieve it for you later." He understands the risks so he clicks Yes.
