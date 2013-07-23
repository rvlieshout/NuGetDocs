## Auth, Enterprise and Social Stories
Some longish user stories about authentication, enterprise use and social...ness.

**NOTE**: These stories _do_ stretch a little farther than just auth. This is intentional to try and gather more data on the end goals for a richer authentication model.

### Frank gets a package
Frank doesn't really _like_ NuGet. It's just something he has to live with. His boss told him to use it so he does. Frank goes to his boss one day and asks him "can we use the latest version of jQuery for our login page?". Frank's boss responds "sure, but use NuGet to install it.". Frank sighs and heads back to his office. He opens VS and pulls up NuGet. He searches for 'jQuery' and clicks Install. The install finishes and Frank thinks to himself _"wow, that was actually easier than I thought it would be"_.

**Key Takeaway**: Frank doesn't have to log in!

### Dora explores the packages
Dora likes NuGet. It's a cool ecosystem and it helps her get work done quickly. However, she's a little annoyed at how _much_ exploring she has to do. She'd much rather be able to easily remember the things she's installed, and find packages used by other people she trusts. So, she goes to NuGet.org. On the home page, she finds that she can log in with her Microsoft Account. She's never logged in to NuGet before, so she logs in with her Microsoft Account.

Now she sees a NuGet.org experience tailored for her. So far, there's nothing new there, but now each package has a "favorite" button that allows her to mark it as a favorite package. She's also able to see references other users and "follow" them. She follows a couple of her colleagues as well as some tech "celebs" like Steve Gretelman (a well known web developer in the industry). As she follows packages, the home page changes. Now, it shows a list of "packages for you" which shows packages she's favorited and packages favorited by the users she follows. When she goes to the main list, she sees notes like "3 of your friends like this package" beside each package.

She switches over to Visual Studio to install some packages. When she opens the "Manage NuGet Packages" dialog, it shows a list sorted by search relevance, but with "favorite" and "followee favorites" factored in. Packages she likes and packages liked by the users she follows bubble to the top of the list, but don't block out other relevant packages. She installs a bunch of packages and continues working. Because Dora is logged in to VS with the same Microsoft Account she used on NuGet.org, she's automatically logged in there as well.

Over the next few weeks and months, as Dora uses more packages, she notices her feed change. Even packages she hasn't favorited yet start bubbling to the top of her feeds because she uses them a lot. Also, as her friends continue finding and installing new packages, those packages start to pop up on her radar when searching for things. Dora is excited by just how much NuGet is helping her explore new tools.

**Key Takeaway**: Logging in to NuGet, on both the Gallery and the Client, activates a new more social experience.

### Dave makes a package
Dave loves NuGet. He's the main author on a fairly large open source project called "DSON.Net" (for Dave'S Object Notation in .NET). He's had an account on NuGet.org from the beginning, but today when he went to check out the latest stats on his package, he saw a new "Sign in with Microsoft Account" option. He clicks it and signs in. When he is returned to NuGet.org a large on-screen message tells him that a new account has been created, but that if he already had an account he can merge it. He chooses the "merge" option and enters his old username and password. The next page asks him if he wants to remove the password for his account (and use Microsoft Account only). Dave loves NuGet... but Dave's also afraid of too much change, so he decides to keep the password on his account. The site detects that Dave owns some packages and directs him to a new documentation page with some short tips for working with a Microsoft Account.

Stretch Idea:
>On this new page, Dave discovers that if he types "nuget login" at the command line, he can enter in his Microsoft Account credentials, and no longer has to use an API key (though the API key still works). After that, future "nuget push" commands will just use his stored Microsoft Account credential. In fact, if he just pushes without logging in OR specifying an API key, 'nuget.exe' will prompt him to log in using either his username/password, Microsoft Account or API Key (all over **SSL only**).

Dave logs in to VS with the same Microsoft Account, and now he sees new options to push packages directly from VS! (future idea, subject to significant design)

Because Dave already used setAPIKey or the '-ApiKey' command line parameter during his build process, his build process doesn't change.

**Key Takeaway**: Microsoft Account/External login doesn't affect key scenarios.

### Frank is forced to push a package
_"Oh no"_ is Frank's first thought when he gets an email from his manager:

> Hey Frank, can you set up a team account on NuGet.org and publish our FooWare SDK packages there? 
> Dave already build some, but he's really busy and needs you to push them. They're up on the build share, all you have to do is push them to NuGet.org. Use your FooWare account, we've already set up Federation with them.

_"What? I don't want to learn all this complicated NuGet stuff."_ is his second thought.

So, Frank gets to work, and the first thing he does is go to NuGet.org. There's a big "Publish Packages" button on the front page that tells him to click it for all the information he needs to publish packages. So he does that. On that page is a set of scenarios with links for more information:

1. Uploading a personal package
2. Setting up a team account and pushing team packages

He clicks on the "team account" link for more information. On there, he sees some simple instructions, and he follows them. First, he logs in to NuGet.org. Frank uses his FooWare account "frank@fooware.biz". The login process succeeds automatically without him ever entering a password (he's already logged in to FooWare's STS). _"Huh... that wasn't so bad I guess"_ he thinks. 

Now he's back at the same page, but this time, there's a big button that says "Create Team Account", along with some text. The text indicates that a Team Account is an account which doesn't actually have it's own login information. Instead, he can just give other NuGet.org users access to the account. That sounds like what he wants, so he clicks it. The next page asks him for a name, and gives him a form to enter other people to "own" the account. The form says he can enter user names or email address, so he enters his boss's FooWare email address in the field "rob@fooware.biz". A message appears saying Rob will get an email with more information. Frank notices that his email address is already in the list of owners and it can't be removed. He clicks the submit button and is take back to the original instructions page. Only now, there's a popup message pointing out that the text field at the top right with his user name in it is now a dropdown and that he can use that dropdown to switch accounts. He opens the dropdown and sees the "FooWare" account he just created.

The instructions immediately change and now show him instructions on downloading NuGet.exe and the following message:

> To push packages using this team account, use this command:
>> nuget push -apikey {123456...} C:\path\to\package.nupkg

(Where the API Key shows is the one for the "FooWare" account). There's some other information about managing Team Accounts, but he leaves that for Rob and Dave to figure out. He pushes the packages using the command and emails Rob:

> Done. I created the "FooWare" account and gave you access. I'll leave it to you and Dave to figure out where we go from there.

Frank pushes back from his desk and sighs _"That was fairly painless I guess"_, and he goes and gets a coffee... and maybe a chocolate bar to reward himself.

**Key Takeaways**: Painless configuration with simple guidance. First class Team Accounts. Corporate Federation makes enterprise usage trivial.

### Dave federates ALL THE THINGS
Dave love Federation almost as much as NuGet. He wants his whole company to use NuGet and he wants it to be easy. So, he goes to NuGet.org and on the home page sees a page about "Enterprise Users" with all sorts of fun buzzwords like Federation and Policy. Dave loves buzzwords. He clicks the link and is taken to a page with information about features NuGet has for Enterprises. On there he sees a "Let your users login to NuGet using your existing accounts" header describing federation. _"Perfect"_ he thinks. By following the wizard process he's able to input the necessary federation data and create a federation trust with NuGet.org. Now his entire corporate network is filled with brand new NuGet users!

**Key Takeway**: Self-service Federation is a stretch goal that would be awesome :)
