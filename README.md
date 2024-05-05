# devtool

## init proj
```bash
# install AntDesign.Templates
dotnet new --install AntDesign.Templates
# create proj. dir
dotnet new antdesign --host='server' -o devtool
```

## add db support
```bash
docker run -d --name=postgres13 -p 5432:5432 -v postgres-volume:/var/lib/postgresql/data -e POSTGRES_PASSWORD=123456 postgres
```
连接数据库，默认库 postgre 默认用户名 postgre 密码上述写的这个


## add dbcontext into blazor 
```bash
dotnet tool install --global dotnet-ef
```