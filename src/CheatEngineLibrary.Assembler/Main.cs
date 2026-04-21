using static CheatEngine.CheatEngineLibrary;

namespace Assembler;

public partial class fmSample : Form
{
    public fmSample()
    {
        InitializeComponent();
    }

    void BtnLoad_Click(object sender, EventArgs e)
    {
        LoadEngine();
    }

    void BtnUnload_Click(object sender, EventArgs e)
    {
        UnloadEngine();
    }

    void BtnProcesses_Click(object sender, EventArgs e)
    {
        var dict = new SortedDictionary<int, string>();
        ltBox.Items.Clear();
        GetProcessList2(line =>
        {
            var pid = TryGetProcessId(line, out var pid2) ? pid2 : default;
            var line2 = $"{pid}-{line}";
            dict.Add(pid, line2);
        });
        foreach (var it in dict)
        {
            ltBox.Items.Add(it.Value);
        }
    }

    void BtnOpenProcess_Click(object sender, EventArgs e)
    {
        var pid = ltBox.SelectedItem?.ToString();
        if (string.IsNullOrWhiteSpace(pid))
        {
            return;
        }
        pid = GetHexProcessId(pid);
        if (pid != null)
        {
            OpenProcess(pid);
            MessageBox.Show("Process opened");
        }

    }

    void BtnInject_Click(object sender, EventArgs e)
    {
        AddScript("example", tbScript.Text);
        ActivateRecord(0, true);
    }
}
