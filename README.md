![SelfInitializing Fakes logo](assets/selfinitializingfakes_128x128.png)

# SelfInitializingFakes

A framework for creating [self-initializing fakes](https://martinfowler.com/bliki/SelfInitializingFake.html).
To quote Martin Fowler,

> The first time you call the fake it passes the call onto the actual remote service, and as it returns the
> data it takes and saves a copy. Further calls just return the copy.

The self-initializing fakes are not intended to be used as general-purpose, configurable dynamic fakes.
Instead, once they are created, and have recorded a sequence of calls against a real service, they
expect to encounter those same calls again, and will return the same results.

This package was created to provide the same functionality as the self-initializing fakes in [FakeItEasy](https://fakeiteasy.github.io), which were
recently deprecated from the .NET 4.0 version of the library and do not exist at all in the .NET Standard version.

## Highlights

* only requires a "real" service to exist when recording
* built-in binary (.NET Framework only) and XML serializers
* user-supplied serializers can provide flexible storage

## Usage

Assuming a class `Service` that implements `IService`, a self-initializing fake version could be used like so:

```c#
var callRepository = new XmlFileRecordedCallRepository("calls.xml");
using (var selfInitializingService = SelfInitializingFake.For<IService>(() => new Service(), callRepository))
{
    var systemUnderTest = new SystemUnderTest(selfInitializingService.Fake);
    systemUnderTest.DoSomething(); // internally exercises selfInitializingService.Fake
}
```

Let's examine this line by line (excluding the braces). The first time this code is run, it:

1. Creates a new call repository that will save recorded calls in the file "calls.xml".
2. Creates a new self-initializing fake. Because the call repository has never been filled, this self-initializing fake wraps the `new Service()` object.
3. This is just a line with a brace. It doesn't do much.
4. Obtains a fake object (of type `IService`) from the self-initializing fake and injects it into a new system under test object.
5. Exercises the system under test, which will use the `selfInitializingService.Fake` object as a collaborator. All calls made to `selfInitializingService.Fake` will be forwarded to the concrete `Service` object.
6. Disposes of the `selfInitializingService`, which will cause it to save all calls made to it (and responses) to the `callRepository`.

So far, the self-initializing fake has provided no benefit; a real `Service` object was used, and did all the work it would normally do.
In order to realize a benefit, "calls.xml" should be retained (ideally placed under version control) for subsequent test runs.
Then when the test is run again, the code:

1. Creates a new call repository that will load recorded calls from the file "calls.xml".
2. Creates a new self-initializing fake. Because the call repository was previously filled with calls, this self-initializing fake does not need to create a `new Service()`.
3. This is just a line with a brace. It doesn't do much.
4. Obtains a fake object (of type `IService`) from the self-initializing fake and injects it into a new system under test object.
5. Exercises the system under test, which will use the `selfInitializingService.Fake` object as a collaborator. All calls made to `selfInitializingService.Fake` will be handled by the object-faking mechanism.
6. Disposes of the `selfInitializingService`, which does nothing, because there's no need to save the calls to "calls.xml".

If at some point the interactions with `Service` need to change, or a real-life `Service` is found to behave differently
than it used to, the old call repository ("calls.xml" in this example) can be removed. Then a new test run can
regenerate it and it's good to use thereafter.

----
Logo: [Self Obsession](https://thenounproject.com/search/?q=self+obsession&i=54849])
by [Michael A. Salter](https://thenounproject.com/michael.salter.73/) from the Noun Project.
