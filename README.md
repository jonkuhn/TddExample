# TDD Example
This repository is an example of doing Test Driven Development (TDD) in C# using [NUnit](https://nunit.org/) and [NSubstitute](https://nsubstitute.github.io/).

## Purpose
My main reason for doing this example is to give myself example code to draw from for writing [blog posts](http://jonkuhn.com/) about TDD and Unit Testing.  I put it in a public repository in case it is also useful to others as an example of the process of TDD.

## Commit Structure
In order to demonstrate how I do TDD, this repository is structured such that there is one commit for each phase of the process.  This means that one commit will add a test and the next commit will make it pass.  I also use commit messages to explain my reasoning at the time for doing what I did.

## What is the example?
For this TDD example I wrote the business logic for checking out a book from a library that has the following requirements:
- If checking out another book would put the member over the maximum number of checked out books, do not allow the book to be checked out.
- If the member has past due books, do not allow the book to be checked out.
- If no copy of the book is still available, the member cannot check out the book.
- If checking out a book is allowed and a copy is available, record a copy of the book as checked out by the member.
- If a copy of the book was successfully checked out, call out to a reminder service that will remind the member when the book is due.

