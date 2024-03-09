### Removed

- Support for FakeItEasy < 8.0.0
- Support for net40, netstandard1.6 TFMs

### New

- Support FakeItEasy 8.0.0+

## 0.6.0

### New

- Allow multiple path components in file-based recorded call repository constructors ([#88](https://github.com/blairconrad/SelfInitializingFakes/pull/88))
- Create missing directories for file-based recorded call repositories ([#87](https://github.com/blairconrad/SelfInitializingFakes/pull/87))

### With special thanks for contributions to this release from:
- [CableZa](https://github.com/CableZa)

## 0.5.0

### New

- Provide netstandard2.0 package (restricted to FakeItEasy 4.9.2+)
  ([#82](https://github.com/blairconrad/SelfInitializingFakes/pull/82))

## 0.4.0

### New

- Support FakeItEasy 5.* and 6.*
  ([#69](https://github.com/blairconrad/SelfInitializingFakes/pull/69), [#73](https://github.com/blairconrad/SelfInitializingFakes/pull/73))

### Changed

- Require faked services to be reference types, enforced by compiler
  ([#68](https://github.com/blairconrad/SelfInitializingFakes/pull/68))
- `RecordedCall` no longer has a public constructor ([#77](https://github.com/blairconrad/SelfInitializingFakes/pull/77))
- Expose null reference type annotations ([#78](https://github.com/blairconrad/SelfInitializingFakes/pull/78))

### Additional Items

- Fixed link to Self Obsession logo ([#70](https://github.com/blairconrad/SelfInitializingFakes/pull/70))
- Use [Bullseye](https://github.com/adamralph/bullseye) 2.3.0 to provide coloured build output on Windows ([#59](https://github.com/blairconrad/SelfInitializingFakes/pull/59), [#67](https://github.com/blairconrad/SelfInitializingFakes/pull/67))
- Switch build script from PowerShell to cmd ([#60](https://github.com/blairconrad/SelfInitializingFakes/pull/60))
- Update building instructions ([#71](https://github.com/blairconrad/SelfInitializingFakes/pull/71))

### With special thanks for contributions to this release from:
- [Adam Ralph](https://github.com/adamralph)
- [Cédric Luthi](https://github.com/0xced)

## 0.3.0

### Changed
- `SelfInitializingFake<TService>` is now sealed ([#50](https://github.com/blairconrad/SelfInitializingFakes/pull/50))

### Fixed
- Bad dependency on  FakeItEasy.Analyzer ([#54](https://github.com/blairconrad/SelfInitializingFakes/pull/54))

### Additional Items
- Use [Bullseye](https://github.com/adamralph/bullseye) and [SimpleExec](https://github.com/adamralph/simple-exec) in build script
  ([#51](https://github.com/blairconrad/SelfInitializingFakes/pull/51),
   [#52](https://github.com/blairconrad/SelfInitializingFakes/pull/52),
   [#56](https://github.com/blairconrad/SelfInitializingFakes/pull/56))

### With special thanks for contributions to this release from:
- [Adam Ralph](https://github.com/adamralph)

## 0.2.1

### Fixed

- Error during recording throws a PlaybackException ([#48](https://github.com/blairconrad/SelfInitializingFakes/issues/48))

### With special thanks for contributions to this release from:
- [Thomas Levesque](https://github.com/thomaslevesque)

## 0.2.0

### Changed
- Require faked service type to be mentioned explicitly to avoid accidentally creating live services ([#37](https://github.com/blairconrad/SelfInitializingFakes/issues/37))

### New
- Support FakeItEasy [3.0.0, 5.0.0) ([#27](https://github.com/blairconrad/SelfInitializingFakes/issues/27))

### Additional Items
- Explicitly use SDK 1.0.4 to build ([#32](https://github.com/blairconrad/SelfInitializingFakes/issues/32))
- Use simple-targets-csx 6.0.0 ([#33](https://github.com/blairconrad/SelfInitializingFakes/issues/33))

### With special thanks for contributions to this release from:
- [Adam Ralph](https://github.com/adamralph)

## 0.2.0-beta003

### Changed
- Require faked service type to be mentioned explicitly to avoid accidentally creating live services ([#37](https://github.com/blairconrad/SelfInitializingFakes/issues/37))

### New
- Support FakeItEasy [3.0.0, 5.0.0) ([#27](https://github.com/blairconrad/SelfInitializingFakes/issues/27))

### Additional Items
- Explicitly use SDK 1.0.4 to build ([#32](https://github.com/blairconrad/SelfInitializingFakes/issues/32))
- Use simple-targets-csx 6.0.0 ([#33](https://github.com/blairconrad/SelfInitializingFakes/issues/33))

### With special thanks for contributions to this release from:
- [Adam Ralph](https://github.com/adamralph)

## 0.2.0-beta001

Failed release. Never made it to NuGet.

## 0.2.0-beta001

Failed release. Never made it to NuGet.

## 0.1.0

- Initial release. Aims to provide all the functionality of the "SelfInitializedFakes" in FakeItEasy 3.0.0.
