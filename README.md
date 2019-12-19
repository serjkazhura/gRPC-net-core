# gRPC-net-core
sample gRPC implementation for .net core and python

## Setup

### Build the Database

If you don't have the Entity Framework Core tools installed for .NET 3.0, you'll need to:

```
C:/>dotnet tool install --global dotnet-ef
```

Then you can build the database by running this in the root of the project (at a console):

```
C:/>dotnet ef database update
```

### Build the Vue Project

Then you'll need to ensure you have the Vue CLI installed. Open a console and type:

```
C:/>npm install @vue/cli -g
```

Once that is installed, you can build the vue project by opening a console in the root of the source folder and typing:

```
C:/>npm run build
```

You could also watch changes to the Vue project by calling:

```
C:/>npm run watch
```
