using System;
using NLog.Web;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace BlogsConsole
{
    class Program
    {
        // create static instance of Logger
        private static NLog.Logger logger = NLogBuilder.ConfigureNLog(Directory.GetCurrentDirectory() + "\\nlog.config").GetCurrentClassLogger();
        static void Main(string[] args)
        {
            logger.Info("Program started");

            try
            {
                string choice;
                do {
                    Console.WriteLine("Select an option");
                    Console.WriteLine("1) Display All Blogs");
                    Console.WriteLine("2) Add Blog");
                    Console.WriteLine("3) Create Post");
                    Console.WriteLine("4) Display Post(s)");
                    Console.WriteLine("Enter q to quit");
                    choice = Console.ReadLine();
                    Console.Clear();
                    logger.Info($"User selected option {choice}");

                    if (choice == "1") {
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.Name);
                        Console.WriteLine($"Found {query.Count()} blogs");
                        foreach (var item in query)
                        {
                            Console.WriteLine(item.Name);
                        }
                    } else if (choice == "2") {
                        Console.Write("Enter a name for a new Blog: ");
                        string name = Console.ReadLine();

                        var blog = new Blog { Name = name };

                        ValidationContext context = new ValidationContext(blog, null, null);
                        List<ValidationResult> results = new List<ValidationResult>();

                        var isValid = Validator.TryValidateObject(blog, context, results, true);
                        if (isValid)
                        {
                            var db = new BloggingContext();
                            // check for unique name
                            if (db.Blogs.Any(b => b.Name == blog.Name))
                            {
                                // generate validation error
                                isValid = false;
                                results.Add(new ValidationResult("Blog name exists", new string[] { "Name" }));
                            }
                            else
                            {
                                logger.Info("Validation passed");
                                // save blog to db
                                db.AddBlog(blog);
                                logger.Info("Blog added - {name}", blog.Name);
                            }
                        }
                        if (!isValid)
                        {
                            foreach (var result in results)
                            {
                                logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                            }
                        }
                    } else if (choice == "3") {
                        Console.WriteLine("What blog would you like to post?");
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.BlogId);
                        int tmpNum = 1;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{tmpNum}) {item.Name}");
                            tmpNum++;
                        }
                        int bchoice = int.Parse(Console.ReadLine());
                        if (bchoice >= 1 || bchoice <= tmpNum) {
                            Console.Clear();
                            
                            var post = new Post { BlogId = bchoice-1 };
                            post.Blog = query.ToList()[bchoice-1];

                            logger.Info($"User selected {query.ToList()[bchoice-1].Name}");
                            Console.WriteLine($"Selected: {query.ToList()[bchoice-1].Name}");

                            Console.WriteLine("Enter post title:");
                            string pTitle = Console.ReadLine();
                            post.Title = pTitle;

                            Console.WriteLine("Enter context of post:");
                            string pContent = Console.ReadLine();
                            post.Content = pContent;

                            ValidationContext context = new ValidationContext(post, null, null);
                            List<ValidationResult> results = new List<ValidationResult>();

                            var isValid = Validator.TryValidateObject(post, context, results, true);
                            if (isValid)
                            {
                                db = new BloggingContext();
                                // check for unique name
                                if (db.Posts.Any(p => p.Title == post.Title))
                                {
                                    // generate validation error
                                    isValid = false;
                                    results.Add(new ValidationResult("Post name exists", new string[] { "Name" }));
                                }
                                else
                                {
                                    logger.Info("Validation passed");
                                    // save post to db
                                    db.AddPost(post);
                                    logger.Info("Post added - {name}", post.Title);
                                }
                            }
                            if (!isValid)
                            {
                                foreach (var result in results)
                                {
                                    logger.Error($"{result.MemberNames.First()} : {result.ErrorMessage}");
                                }
                            }
                        } else {
                            Console.WriteLine("Number sumbitted was out of range");
                        }
                    } else if (choice == "4") {
                        Console.WriteLine("What blog would you like to see the posts on?");
                        var db = new BloggingContext();
                        var query = db.Blogs.OrderBy(b => b.BlogId);
                        int tmpNum = 1;
                        foreach (var item in query)
                        {
                            Console.WriteLine($"{tmpNum}) {item.Name}");
                            tmpNum++;
                        }
                        int bchoice = int.Parse(Console.ReadLine());

                        if (bchoice >= 1 || bchoice <= tmpNum) {
                            Console.Clear();
                            Console.WriteLine($"Selected posts with \"{query.ToList()[bchoice-1].Name}\" blog");
                            var query2 = db.Posts.Where(p => p.BlogId == bchoice);
                            Console.WriteLine($"Found {query2.Count()} posts");

                            foreach (var post in query2)
                            {
                                Console.WriteLine($"Blog Name: {post.Blog.Name}");
                                Console.WriteLine($"Post Title: {post.Title}");
                                Console.WriteLine($"Content: {post.Content}");
                                Console.WriteLine("");
                            }
                        } else {
                            Console.WriteLine("Number sumbitted was out of range");
                        }
                    }
                } while (choice.ToLower() != "q");
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message);
            }


            logger.Info("Program ended");
        }
    }
}