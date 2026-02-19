---
title: API Reference
sidebar_position: 1
---

# API Reference

The API reference documentation is auto-generated from XML documentation comments in the source code.

When building locally, run the following to generate the API docs:

```bash
dotnet tool restore
dotnet build -c Release
dotnet xmldoc2md src/Bogoware.Localization/bin/Release/net10.0/Bogoware.Localization.dll -o docs/docs/api --index-page-name index --github-pages
```

In CI, API documentation is generated automatically by the `docs.yml` workflow.
