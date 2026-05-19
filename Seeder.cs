using Microsoft.EntityFrameworkCore;
using social_media_console_app.BusinessLogic.Services;
using social_media_console_app.Data;
using social_media_console_app.Models;
using social_media_console_app.ProjectConstants.Enums;

namespace social_media_console_app;

public class Seeder(PasswordHasher passwordHasher, ApplicationDbContext dbContext)
{
    private static readonly string[] Bios =
    [
        "Just here to connect with people.",
        "Coffee addict and weekend hiker.",
        "Living life one day at a time.",
        "Passionate about music and travel.",
        "Software dev by day, gamer by night.",
        "Dog lover. Pizza enthusiast.",
        "Always chasing the next adventure.",
        "Bookworm and occasional chef.",
        "Fitness junkie. Early riser.",
        "Photography is my therapy.",
        "Making memories, not excuses.",
        "Introvert with extrovert tendencies.",
        "Foodie exploring the city one bite at a time.",
        "Tech nerd. Coffee dependent.",
        "Hiking trails and mountain views.",
        "Art lover. Museum regular.",
        "Remote worker. World traveler.",
        "Dad jokes and dad bods.",
        "Just a girl who loves plants.",
        "Trying to adult. Results vary."
    ];

    private static readonly string[] PostTitles =
    [
        "What a day!",
        "Thoughts on remote work",
        "Weekend recap",
        "Just got back from a hike",
        "Finally tried that new restaurant",
        "Random shower thoughts",
        "Currently reading...",
        "Hot take incoming",
        "Things I learned this week",
        "Unpopular opinion",
        "Morning routine update",
        "Question for everyone",
        "Life update",
        "Can't believe it's already Friday",
        "Recommendations please",
        "Productivity hack I found",
        "Just finished this book",
        "Weather today is insane",
        "Looking for hiking buddies",
        "Anyone else notice this?"
    ];

    private static readonly string[] PostContents =
    [
        "Today was one of those days where everything just clicked. Hard to explain but I feel like I'm finally on the right track.",
        "Been working from home for three years and I still can't decide if I love it or hate it. The commute is unbeatable though.",
        "Spent the whole weekend outdoors. Barely touched my phone. Highly recommend the digital detox.",
        "Just finished a 12km trail. Legs are dead but the view from the top made every step worth it.",
        "That new Italian place downtown is absolutely worth the hype. Get the truffle pasta. Trust me.",
        "Why does hot water make you sleepy but hot coffee wake you up? Genuinely thinking about this.",
        "Currently reading Atomic Habits for the second time. Hitting differently now that I actually have habits to reflect on.",
        "Unpopular opinion: open offices are worse for productivity than working from a coffee shop. Fight me.",
        "Three things I learned this week: delegate more, sleep earlier, and reply to messages the same day.",
        "Pineapple on pizza is not the problem. The problem is bad pizza regardless of toppings.",
        "Started waking up at 6am. Two weeks in. I genuinely feel like a different person.",
        "Quick question — do you prefer texting or calling? Asking for science.",
        "Moved to a new apartment last week. Still surrounded by boxes but already love the neighborhood.",
        "How is it Friday again? Time is moving at an unreasonable pace lately.",
        "Looking for book recommendations. Currently into non-fiction but open to anything good.",
        "Found out you can batch-cook rice and freeze it. My weeknight dinners are transformed.",
        "Just finished The Midnight Library. Went in blind, came out emotionally rearranged.",
        "It is somehow both raining and sunny at the same time. The weather has lost the plot.",
        "Looking for someone to join a weekend hike next Saturday. Easy trail, great views, good company.",
        "Am I the only one who refreshes their inbox right after sending an important email?"
    ];

    private static readonly string[] CommentContents =
    [
        "This is so relatable!",
        "Couldn't agree more.",
        "Had the exact same experience last week.",
        "I needed to read this today.",
        "Wait, which trail was this?",
        "Okay now I have to try that restaurant.",
        "Sending this to everyone I know.",
        "The last point especially. So true.",
        "I've been thinking about this too!",
        "Valid. Absolutely valid.",
        "How long did the hike take you?",
        "Same. Every single time.",
        "Okay hot take but I agree.",
        "Which book was it? Asking for a friend.",
        "This is the content I'm here for.",
        "I'm in for the hike if you're still looking!",
        "Underrated opinion honestly.",
        "That restaurant has a long wait but worth it.",
        "The frozen rice thing changed my life too.",
        "100% yes. Always refresh immediately after sending."
    ];

    private static readonly string[][] Conversations =
    [
        [
            "Hey! How have you been?",
            "Pretty good! Busy week but can't complain. You?",
            "Same honestly. Finally feeling settled.",
            "That's good to hear. We should catch up properly soon.",
            "Definitely. Coffee this weekend?"
        ],
        [
            "Did you see the post about the hike?",
            "Yeah! I'm actually thinking of going.",
            "Me too. Want to go together?",
            "100%. Let's plan it.",
            "Saturday morning works for me.",
            "Perfect. I'll bring snacks."
        ],
        [
            "That new restaurant is unreal.",
            "I know right! The pasta was incredible.",
            "We need to go back.",
            "Already thinking about it honestly."
        ],
        [
            "Can you believe the weather today?",
            "It's been raining since Tuesday I think.",
            "I've given up checking the forecast.",
            "Same. Just assuming it'll be bad and being pleasantly surprised.",
            "Smart system.",
            "Survival strategy at this point."
        ],
        [
            "Hey, quick question — do you use any productivity apps?",
            "I've been using Notion for everything lately.",
            "How do you find it?",
            "Takes a bit to set up but once it clicks it's great.",
            "Might give it a shot. Thanks!",
            "Let me know what you think."
        ],
        [
            "Happy Friday!",
            "Finally!! This week was rough.",
            "Tell me about it. Any plans?",
            "Absolutely nothing and I'm thrilled about it.",
            "That sounds perfect honestly."
        ],
        [
            "Just finished The Midnight Library.",
            "Oh wow how was it?",
            "Absolutely wrecked me. In a good way.",
            "I've had it on my list for ages.",
            "Move it to the top. Trust me.",
            "Okay okay I'll start it this weekend.",
            "Good. We can talk about it after."
        ],
        [
            "Morning!",
            "Morning. Coffee acquired?",
            "Two cups in. Ready for anything.",
            "Respect. I'm still on cup one.",
            "You'll get there.",
            "The dream."
        ]
    ];

    public async Task SeedAsync()
    {
        bool hasUsers = await dbContext.Users.AnyAsync();

        if (!hasUsers)
        {
            // ── Users ─────────────────────────────────────────────────────────
            // Hash once and reuse — all seeded users share the same password
            var (hash, salt) = passwordHasher.HashPassword("password123");

            var userData = new (string username, string email, string? bio, DateTime dob)[]
            {
                ("alice",   "alice@example.com",   Bios[0],  new DateTime(1995, 4,  12)),
                ("boba",    "bob@example.com",      Bios[1],  new DateTime(1992, 8,  3)),
                ("carol",   "carol@example.com",    Bios[2],  new DateTime(1998, 1,  22)),
                ("dave",    "dave@example.com",     null,     new DateTime(1990, 11, 5)),
                ("emma",    "emma@example.com",     Bios[4],  new DateTime(1997, 3,  18)),
                ("frank",   "frank@example.com",    Bios[5],  new DateTime(1988, 7,  9)),
                ("grace",   "grace@example.com",    Bios[6],  new DateTime(2000, 2,  14)),
                ("henry",   "henry@example.com",    Bios[7],  new DateTime(1993, 9,  27)),
                ("iris",    "iris@example.com",     Bios[8],  new DateTime(1996, 6,  3)),
                ("jake",    "jake@example.com",     Bios[9],  new DateTime(1991, 12, 1)),
                ("karen",   "karen@example.com",    Bios[10], new DateTime(1994, 5,  20)),
                ("liam",    "liam@example.com",     Bios[11], new DateTime(1999, 10, 8)),
                ("mia",     "mia@example.com",      Bios[12], new DateTime(2001, 1,  30)),
                ("noah",    "noah@example.com",     Bios[13], new DateTime(1990, 4,  15)),
                ("olivia",  "olivia@example.com",   Bios[14], new DateTime(1997, 8,  22)),
                ("peter",   "peter@example.com",    Bios[15], new DateTime(1985, 3,  11)),
                ("quinn",   "quinn@example.com",    Bios[16], new DateTime(2002, 7,  4)),
                ("rachel",  "rachel@example.com",   Bios[17], new DateTime(1989, 11, 19)),
                ("sam",     "sam@example.com",      Bios[18], new DateTime(1996, 9,  6)),
                ("tara",    "tara@example.com",     Bios[19], new DateTime(1993, 2,  28)),
                ("uma",     "uma@example.com",      Bios[0],  new DateTime(1998, 6,  14)),
                ("victor",  "victor@example.com",   Bios[1],  new DateTime(1987, 4,  3)),
                ("wendy",   "wendy@example.com",    Bios[2],  new DateTime(2000, 12, 25)),
                ("xavier",  "xavier@example.com",   Bios[3],  new DateTime(1995, 7,  17)),
                ("yara",    "yara@example.com",     Bios[4],  new DateTime(1999, 3,  9)),
                ("zack",    "zack@example.com",     Bios[5],  new DateTime(1991, 1,  21)),
                ("amber",   "amber@example.com",    Bios[6],  new DateTime(1994, 8,  13)),
                ("blake",   "blake@example.com",    Bios[7],  new DateTime(1988, 5,  2)),
                ("chloe",   "chloe@example.com",    Bios[8],  new DateTime(2003, 10, 31)),
                ("derek",   "derek@example.com",    null,     new DateTime(1990, 6,  7)),
                ("elena",   "elena@example.com",    Bios[10], new DateTime(1997, 2,  19)),
                ("finn",    "finn@example.com",     Bios[11], new DateTime(1993, 9,  14)),
                ("gina",    "gina@example.com",     Bios[12], new DateTime(2001, 4,  5)),
                ("hugo",    "hugo@example.com",     Bios[13], new DateTime(1986, 7,  23)),
                ("isla",    "isla@example.com",     Bios[14], new DateTime(1999, 11, 10)),
                ("joel",    "joel@example.com",     Bios[15], new DateTime(1992, 3,  28)),
                ("kylie",   "kylie@example.com",    Bios[16], new DateTime(2000, 8,  16)),
                ("leon",    "leon@example.com",     Bios[17], new DateTime(1995, 1,  7)),
                ("maya",    "maya@example.com",     Bios[18], new DateTime(1998, 6,  29)),
                ("nick",    "nick@example.com",     Bios[19], new DateTime(1990, 12, 3)),
                ("penny",   "penny@example.com",    Bios[0],  new DateTime(1996, 4,  18)),
                ("ross",    "ross@example.com",     Bios[1],  new DateTime(1987, 9,  11)),
                ("stella",  "stella@example.com",   Bios[2],  new DateTime(2002, 2,  24)),
                ("tyler",   "tyler@example.com",    Bios[3],  new DateTime(1994, 7,  6)),
                ("ursula",  "ursula@example.com",   Bios[4],  new DateTime(1989, 11, 30)),
                ("vince",   "vince@example.com",    Bios[5],  new DateTime(1997, 5,  15)),
                ("willa",   "willa@example.com",    Bios[6],  new DateTime(2001, 1,  9)),
                ("xander",  "xander@example.com",   Bios[7],  new DateTime(1993, 8,  22)),
                ("zoe",     "zoe@example.com",      Bios[8],  new DateTime(1998, 3,  4)),
                ("aaron",   "aaron@example.com",    null,     new DateTime(1991, 6,  17)),
                ("bella",   "bella@example.com",    Bios[10], new DateTime(2000, 10, 8)),
                ("caleb",   "caleb@example.com",    Bios[11], new DateTime(1996, 2,  13)),
                ("diana",   "diana@example.com",    Bios[12], new DateTime(1988, 7,  26)),
                ("ethan",   "ethan@example.com",    Bios[13], new DateTime(2003, 4,  1)),
                ("fiona",   "fiona@example.com",    Bios[14], new DateTime(1995, 9,  19)),
                ("george",  "george@example.com",   Bios[15], new DateTime(1990, 1,  4)),
                ("holly",   "holly@example.com",    Bios[16], new DateTime(1997, 6,  28)),
                ("ivan",    "ivan@example.com",     Bios[17], new DateTime(1993, 11, 12)),
                ("julia",   "julia@example.com",    Bios[18], new DateTime(2001, 3,  25)),
                ("kevin",   "kevin@example.com",    null,     new DateTime(1989, 8,  7)),
                ("laura",   "laura@example.com",    Bios[0],  new DateTime(1994, 5,  16)),
                ("matt",    "matt@example.com",     Bios[1],  new DateTime(1992, 10, 30)),
                ("nora",    "nora@example.com",     Bios[2],  new DateTime(1999, 2,  11)),
                ("oscar",   "oscar@example.com",    Bios[3],  new DateTime(1987, 7,  24)),
                ("paula",   "paula@example.com",    Bios[4],  new DateTime(2002, 1,  8)),
                ("ryan",    "ryan@example.com",     Bios[5],  new DateTime(1995, 6,  21)),
                ("sarah",   "sarah@example.com",    Bios[6],  new DateTime(1991, 3,  14)),
                ("tom",     "tom@example.com",      Bios[7],  new DateTime(1998, 8,  27)),
                ("ulrich",  "ulrich@example.com",   null,     new DateTime(1986, 12, 10)),
                ("vera",    "vera@example.com",     Bios[9],  new DateTime(2000, 4,  23)),
                ("wade",    "wade@example.com",     Bios[10], new DateTime(1993, 9,  5)),
                ("ximena",  "ximena@example.com",   Bios[11], new DateTime(1997, 1,  18)),
                ("yasmin",  "yasmin@example.com",   Bios[12], new DateTime(2004, 5,  1)),
                ("zeus",    "zeus@example.com",     Bios[13], new DateTime(1990, 3,  7)),
                ("abby",    "abby@example.com",     Bios[14], new DateTime(1996, 8,  20)),
                ("ben",     "ben@example.com",      Bios[15], new DateTime(1988, 2,  3)),
                ("clara",   "clara@example.com",    Bios[16], new DateTime(2001, 7,  16)),
                ("dan",     "dan@example.com",      null,     new DateTime(1994, 12, 29)),
                ("eve",     "eve@example.com",      Bios[18], new DateTime(1999, 5,  12)),
                ("felix",   "felix@example.com",    Bios[19], new DateTime(1991, 10, 25)),
                ("greta",   "greta@example.com",    Bios[0],  new DateTime(1997, 4,  8)),
                ("hank",    "hank@example.com",     Bios[1],  new DateTime(1985, 9,  21)),
                ("irene",   "irene@example.com",    Bios[2],  new DateTime(2002, 2,  14)),
                ("josh",    "josh@example.com",     Bios[3],  new DateTime(1993, 7,  27)),
                ("kim",     "kim@example.com",      Bios[4],  new DateTime(1998, 1,  10)),
                ("luke",    "luke@example.com",     Bios[5],  new DateTime(1990, 6,  23)),
                ("molly",   "molly@example.com",    Bios[6],  new DateTime(2000, 11, 5)),
                ("neil",    "neil@example.com",     Bios[7],  new DateTime(1995, 3,  19)),
                ("opal",    "opal@example.com",     Bios[8],  new DateTime(1992, 8,  2)),
                ("phil",    "phil@example.com",     null,     new DateTime(1987, 1,  15)),
                ("queen",   "queen@example.com",    Bios[10], new DateTime(2003, 6,  1)),
                ("rita",    "rita@example.com",     Bios[11], new DateTime(1996, 11, 11)),
                ("steve",   "steve@example.com",    Bios[12], new DateTime(1989, 4,  24)),
                ("tina",    "tina@example.com",     Bios[13], new DateTime(1994, 9,  7)),
                ("ugo",     "ugo@example.com",      Bios[14], new DateTime(2001, 2,  20)),
                ("val",     "val@example.com",      Bios[15], new DateTime(1997, 7,  3)),
                ("will",    "will@example.com",     Bios[16], new DateTime(1991, 12, 16)),
                ("xena",    "xena@example.com",     Bios[17], new DateTime(1999, 5,  30)),
                ("yvonne",  "yvonne@example.com",   Bios[18], new DateTime(1988, 10, 12)),
                ("zara",    "zara@example.com",     Bios[19], new DateTime(2002, 3,  25)),
            };

            var users = userData.Select(u => new User
            {
                Username     = u.username,
                Email        = u.email,
                Bio          = u.bio,
                DateOfBirth  = u.dob,
                PasswordHash = hash,
                PasswordSalt = salt
            }).ToList();

            await dbContext.Users.AddRangeAsync(users);
            await dbContext.SaveChangesAsync();

            // ── Friendships ───────────────────────────────────────────────────
            var usedPairs   = new HashSet<(int, int)>();
            var friendships = new List<Friendship>();

            void AddFriendship(int a, int b, FriendshipStatus status)
            {
                var key = (Math.Min(a, b), Math.Max(a, b));
                if (usedPairs.Add(key))
                {
                    friendships.Add(new Friendship
                    {
                        RequesterUserId  = users[a].Id,
                        AddresseeUserId  = users[b].Id,
                        FriendshipStatus = status
                    });
                }
            }

            // Ring: each user is accepted friends with the next 3 users (wrapping)
            for (int i = 0; i < users.Count; i++)
            {
                for (int j = 1; j <= 3; j++)
                {
                    AddFriendship(i, (i + j) % users.Count, FriendshipStatus.Accepted);
                }
            }

            // Cross-cluster accepted connections at distance 20 and 40
            for (int i = 0; i < users.Count; i += 5)
            {
                AddFriendship(i, (i + 20) % users.Count, FriendshipStatus.Accepted);
                AddFriendship(i, (i + 40) % users.Count, FriendshipStatus.Accepted);
            }

            // Pending requests at distance 7 (outside ring range of 3)
            for (int i = 2; i < users.Count; i += 5)
            {
                AddFriendship(i, (i + 7) % users.Count, FriendshipStatus.Pending);
            }

            // A few declined requests
            AddFriendship(0,  50, FriendshipStatus.Declined);
            AddFriendship(10, 60, FriendshipStatus.Declined);
            AddFriendship(20, 70, FriendshipStatus.Declined);

            await dbContext.Friendships.AddRangeAsync(friendships);
            await dbContext.SaveChangesAsync();

            // ── Posts ─────────────────────────────────────────────────────────
            var posts = new List<Post>();
            for (int i = 0; i < users.Count; i++)
            {
                int postCount = i % 3 == 0 ? 3 : i % 3 == 1 ? 2 : 1;
                for (int p = 0; p < postCount; p++)
                {
                    posts.Add(new Post
                    {
                        UserId      = users[i].Id,
                        PostTitle   = PostTitles[(i + p) % PostTitles.Length],
                        PostContent = PostContents[(i + p) % PostContents.Length]
                    });
                }
            }

            await dbContext.Posts.AddRangeAsync(posts);
            await dbContext.SaveChangesAsync();

            // ── Comments ──────────────────────────────────────────────────────
            var acceptedFriendIds = new Dictionary<int, List<int>>();
            foreach (var u in users)
            {
                acceptedFriendIds[u.Id] = new List<int>();
            }

            foreach (var f in friendships.Where(f => f.FriendshipStatus == FriendshipStatus.Accepted))
            {
                acceptedFriendIds[f.RequesterUserId].Add(f.AddresseeUserId);
                acceptedFriendIds[f.AddresseeUserId].Add(f.RequesterUserId);
            }

            var comments = new List<Comment>();
            for (int i = 0; i < posts.Count; i++)
            {
                var post         = posts[i];
                var commenterIds = acceptedFriendIds[post.UserId].Take(3).ToList();

                for (int c = 0; c < commenterIds.Count; c++)
                {
                    comments.Add(new Comment
                    {
                        PostId          = post.Id,
                        CommenterUserId = commenterIds[c],
                        CommentContent  = CommentContents[(i + c) % CommentContents.Length]
                    });
                }
            }

            await dbContext.Comments.AddRangeAsync(comments);
            await dbContext.SaveChangesAsync();

            // ── Messages ──────────────────────────────────────────────────────
            var messages  = new List<Message>();
            var baseTime  = DateTime.UtcNow.AddDays(-14);
            int convIndex = 0;

            foreach (var f in friendships.Where(f => f.FriendshipStatus == FriendshipStatus.Accepted))
            {
                // ~50% of accepted friend pairs get a conversation
                if (convIndex % 2 == 0)
                {
                    var conv    = Conversations[convIndex % Conversations.Length];
                    var msgTime = baseTime.AddHours(convIndex);

                    for (int m = 0; m < conv.Length; m++)
                    {
                        bool senderIsRequester = m % 2 == 0;
                        messages.Add(new Message
                        {
                            SenderUserId   = senderIsRequester ? f.RequesterUserId : f.AddresseeUserId,
                            ReceiverUserId = senderIsRequester ? f.AddresseeUserId : f.RequesterUserId,
                            MessageContent = conv[m],
                            IsRead         = m < conv.Length - 1,
                            CreatedAt      = msgTime.AddMinutes(m * 4)
                        });
                    }
                }

                convIndex++;
            }

            await dbContext.Messages.AddRangeAsync(messages);
            await dbContext.SaveChangesAsync();

            Console.WriteLine("Seed complete.");
            Console.WriteLine($"  Users      : {users.Count}");
            Console.WriteLine($"  Friendships: {friendships.Count} ({friendships.Count(f => f.FriendshipStatus == FriendshipStatus.Accepted)} accepted, {friendships.Count(f => f.FriendshipStatus == FriendshipStatus.Pending)} pending, {friendships.Count(f => f.FriendshipStatus == FriendshipStatus.Declined)} declined)");
            Console.WriteLine($"  Posts      : {posts.Count}");
            Console.WriteLine($"  Comments   : {comments.Count}");
            Console.WriteLine($"  Messages   : {messages.Count}");
            Console.WriteLine($"  Password   : password123 (all users)");
        }
        else
        {
            Console.WriteLine("Database already seeded. Skipping.");
        }
    }
}