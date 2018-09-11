using behaviac;

public class AIWorld
{
    public Workspace workspace = null;

    public AIWorld(string behaviacPath)
    {
        workspace = new behaviac.Workspace();
        workspace.UseIntValue = true;
        workspace.FilePath = behaviacPath;
        workspace.FileFormat = behaviac.Workspace.EFileFormat.EFF_cs;

    }
	
	public void reset()
    {
        workspace.IntValueSinceStartup = 0;
    }


    public void destory()
    {
        workspace.Cleanup();
    }
}