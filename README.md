![SelfInitializing Fakes logo](assets/selfinitializingfakes_128x128.png)

# SelfInitializingFakes

A framework for creating [self-initializing fakes](https://martinfowler.com/bliki/SelfInitializingFake.html).
To quote Martin Fowler,

> The first time you call the fake it passes the call onto the actual remote service, and as it returns the
> data it takes and saves a copy. Further calls just return the copy.

The self-initializing fakes are not intended to be used as general-purpose, configurable dynamic fakes.
Instead, once they are created, and have recorded a sequence of calls against a real service, they
expect to encounter those same calls again, and will return the same results.

Features:

* only requires a "real" service to exist when recoring
* persist call data to files via built-in serializers, or
* user-supplied serializers provide flexible storage

----
Logo: [Self Obsession](https://thenounproject.com/search/?q=self+objsession&i=54849])
by [Michael A. Salter](https://thenounproject.com/michael.salter.73/) from the Noun Project.
