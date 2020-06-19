# How to Contribute

First of all, thank you for wanting to contribute to SelfInitializingFakes! We
want to keep it as easy as possible for you to contribute changes that make
SelfInitializingFakes better for the community, including you. There are a few
guidelines that we need contributors to follow so that we can all work together
happily.

## Preparation

Before starting work on a functional change, i.e. a new feature, a change to an
existing feature or a bug fix, please ensure an
[issue](https://github.com/blairconrad/SelfInitializingFakes/issues) has been
raised. Indicate your intention to work on the issue by writing a comment
against it. This will prevent duplication of effort. If the change is
non-trivial, it's usually best to propose a design in the issue comments.

It is not necessary to raise an issue for non-functional changes, e.g.
refactoring, adding tests, reformatting code, documentation, updating packages,
etc.

## Tests

Changes in functionality (new features, changed behavior, or bug fixes) should
be described by [xBehave.net](https://xbehave.github.io/) acceptance tests in
the `SelfInitializingFakes.Tests.FIE.*` projects (the various projects share a
common set of source tests). Doing so ensures that tests are written in language
familiar to SelfInitializingFakes's end users and are resilient to refactoring.

### API Approval

If your contribution changes the public API, you will initially have a test
failure from the `SelfInitializingFakes.Tests.Api` project. In order to fix
this, check the difference between
`tests\SelfInitializingFakes.Tests.Api\ApprovedApi\SelfInitializingFakes\*.approved.txt`
and
`tests\SelfInitializingFakes.Tests.Api\ApprovedApi\SelfInitializingFakes\*.received.txt`.
If you are satisfied with the changes, update `...approved.txt` to match
`...received.txt`.

## Coding Style

Try to keep your coding style in line with the existing code. It might not
exactly match your preferred style but it's better to keep things consistent.
Coding style compliance is enforced through analysis on each build. Any
StyleCop.Analyzers settings changes or suppressions must be clearly justified.

### Spaces not Tabs

Pull requests containing tabs will not be accepted. Make sure you set your
editor to replace tabs with spaces. Indents for all file types should be 4
characters wide with the exception of JSON which should have indents 2
characters wide.

### Line Endings

The repository is configured to preserve line endings both on checkout and
commit (the equivalent of `autocrlf` set to `false`). This means *you* are
responsible for line endings. We recommend that you configure your diff viewer
so that it does not ignore line endings. Any
[wall of pink](https://www.hanselman.com/blog/YoureJustAnotherCarriageReturnLineFeedInTheWall.aspx)
pull requests will not be accepted.

### Line Width

Try to keep lines of code no longer than 160 characters wide. This isn't a
strict rule. Occasionally a line of code can be more readable if allowed to
spill over slightly.

### String Formatting

Unless there is a compelling reason not to, for example when serializing data to
be parsed later, use the current culture when formatting strings. When string
formatting methods have an overload that implicitly uses the current culture,
opt to use it instead of specifying the culture explicitly.

### Code Analysis

Avoid introducing new code analysis warnings. Currently the codebase is free of
warnings, and we would like to avoid the addition of new warnings. Any code
analysis rule changes or suppressions must be clearly justified.

## Making Changes

SelfInitializingFakes uses the git branching model known as
[GitHub flow](https://help.github.com/articles/github-flow/). As such, all
development must be performed on a
["feature branch"](https://martinfowler.com/bliki/FeatureBranch.html) created
from the main development branch, which is called `master`. To submit a change:

1. [Fork](https://help.github.com/forking/) the
   [SelfInitializingFakes repository](https://github.com/blairconrad/SelfInitializingFakes/)
   on GitHub
1. Clone your fork locally
1. Configure the upstream repo
   (`git remote add upstream git://github.com/blairconrad/SelfInitializingFakes.git`)
1. Create a local branch (`git checkout -b my-branch master`)
1. Work on your feature
1. Rebase if required (see below)
1. Ensure the build succeeds (see ['How to build'](how_to_build.md "How to
   build"))
1. Push the branch up to GitHub (`git push origin my-branch`)
1. Send a [pull request](https://help.github.com/articles/using-pull-requests)
   on GitHub

## Handling Updates from upstream

While you're working away in your branch it's quite possible that your
upstream/master (most likely the canonical SelfInitializingFakes version) may be
updated. If this happens you should:

1. [Stash](https://git-scm.com/book/en/v2/Git-Tools-Stashing-and-Cleaning) any
   un-committed changes you need to
1. `git fetch upstream master`
1. `git rebase upstream/master my-branch`
1. if you previously pushed your branch to your origin, you need to force push
   the rebased branch - `git push origin my-branch --force-with-lease`
1. `git push origin master` - (optional) this makes sure your remote master
   branch is up to date

This ensures that your history is "clean". That is, you have one branch off from
master followed by your changes in a straight line. Failing to do this results
in several "messy" merges in your history, which we don't want. This is the
reason why you should always work in a branch and you should never be working
in, or sending pull requests from, master.

If you're working on a long running feature then you may want to do this quite
often, rather than run the risk of potential merge issues further down the line.

## Sending a Pull Request

While working on your feature you may well create several branches, which is
fine, but before you send a pull request you should ensure that you have rebased
back to a single feature branch. We care about your commits and we care about
your feature branch but we don't care about how many or which branches you
created while you were working on it. :smile:

When you're ready to go you should confirm that you are up to date and rebased
with upstream/master (see "Handling Updates from upstream" above) and
then:

1. `git push origin my-branch`
1. Send a [pull request](https://help.github.com/articles/using-pull-requests)
   in GitHub, selecting the following dropdown values:

| Dropdown      | Value                                                        |
|---------------|--------------------------------------------------------------|
| **base fork** | `blairconrad/SelfInitializingFakes`                          |
| **base**      | `master`                                                     |
| **head fork** | `{your fork}` (e.g. `{your username}/SelfInitializingFakes`) |
| **compare**   | `my-branch`                                                  |

The pull request should include a description starting with "Fixes #123." (using
the real issue number, of course) if it fixes an issue. If there's no issue, be
sure to clearly explain the intent of the change.
