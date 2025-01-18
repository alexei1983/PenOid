# PenOid
Support for interacting with Private Enterprise Number (PEN) object identifiers (OIDs) assigned by IANA.

## About
The Internet Assigned Numbers Authority (IANA) assigns Private Enterprise Numbers (PENs) to organizations and individuals on a first-come, first-served basis. Full details on obtaining a PEN can be found at the [IANA web site](https://www.iana.org/assignments/enterprise-numbers) along with a [list of current registrations](https://www.iana.org/assignments/enterprise-numbers.txt).

The prefix within the OID hierarchy for all PEN assignments is 1.3.6.1.4.1

## Example Usage
Assuming a PEN value of 32473 from IANA, you can interact with the `PenOid` class by creating a new instance:

```
var penOid = new PenOid(32473);
```
Calling `ToString()` on the above instance will yield `1.3.6.1.4.1.32473`.

You can create a new hierarchy beneath the 32473 tree by appending the new identifier to the `PenOid` instance:

```
penOid.AppendComponent(1);
```

Calling `ToString()` on the above instance will now yield: `1.3.6.1.4.1.32473.1`.

You can also create a new `PenOid` instance by providing a full OID in dotted notation:

```
var penOid = new PenOid("1.3.6.1.4.1.32473.1.42");
```

## IANA Registry
This package provides the `PenRegistryProvider` class to download and parse the IANA-maintained PEN registry:

```
var registryProvider = new PenRegistryProvider();
var registryContents = registryProvider.FromIana();
```
Assuming a successful web response, `registryContents` now contains a list of `PenOidWithMetadata` objects representing the IANA-maintained PEN registry.  Each object stores the assigned PEN, registrant name, contact person, and contact detail for a specific PEN in the registry.