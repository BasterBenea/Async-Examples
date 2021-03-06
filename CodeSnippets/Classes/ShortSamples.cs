﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CodeSnippets.Classes
{
    public class ShortSamples
    {
        CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        CancellationToken _cancellationToken = new CancellationToken();

        public async Task Example1Async(PersonArguments personArguments, IProgress<string> progress) 
        {
            Task<Person> taskOne = await Task.Factory.StartNew(async () =>
            {
                var person = new Person() {Id = -1};

                var lines = File.ReadAllLines(personArguments.FileName);
                for (var index = 0; index < lines.Length -1; index++)
                {
                    var lineParts = lines[index].Split(',');
                    if (lineParts[1] == personArguments.FirstName && lineParts[2] == personArguments.LastName)
                    {
                        person.Id = Convert.ToInt32(lineParts[0]);
                        person.Birthday = DateTime.Parse(lineParts[4]);
                        if (lineParts[3] == "1" || lineParts[3] == "2")
                        {
                            person.Gender = lineParts[3] == "1" ? "Female" : "Male";
                        }
                        progress.Report($"Id is {lineParts[0]}");
                        break;
                    }

                    await Task.Delay(1, _cancellationToken);
                }

                progress.Report("Task 1 complete");
                return person;

            }, _cancellationToken);

            var taskTwo = await Task.Factory.StartNew(async () =>
            {
                for (var index = 0; index < 5; index++)
                {
                    _cancellationToken.ThrowIfCancellationRequested();
                    progress.Report(index.ToString());

                    await Task.Delay(12, _cancellationToken);

                    _cancellationToken.WaitHandle.WaitOne(500);
                }

                progress.Report("Task 2 complete");
                return true;

            }, _cancellationToken);

            Task.WaitAll(taskOne, taskTwo);
            progress.Report("Both task done!");


            var personResult = taskOne.Result;

            progress.Report(personResult.Id > -1 ? 
                $"Birthday: {personResult.Birthday:d} - gender: {personResult.Gender}" : 
                $"Not found");
        }
    }
}
