using BHS.Engine.Application;
using BHS.Engine.UI;


if (args.Length > 0 && args[0] == "--gui")
{
    BHS.Engine.UI.Program.Main(args);
}
else
{
    BHS.Engine.Application.Program.Main(args);
}