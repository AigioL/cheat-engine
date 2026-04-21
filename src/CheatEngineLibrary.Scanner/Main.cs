using CheatEngine;
using System.Diagnostics;
using static CheatEngine.CheatEngineLibrary;

namespace Scanner;

public partial class Main : Form
{
    TScanOption scanopt;
    TVariableType varopt;
    bool unicode;
    bool casesensitive;
    string startscan;
    string endscan;

    const int wm_scandone = 0x8000 + 2;

    /// <inheritdoc/>
    protected override void WndProc(ref Message m)
    {
        int size, i;
        if (m.Msg == wm_scandone)
        {
            lvScanner.VirtualListSize = 0;
            lvScanner.Items.Clear();
            btnNextScan.Enabled = true;
            size = GetBinarySize();

            if (varopt == TVariableType.String)
                if (unicode)
                    InitFoundList(varopt, size / 16, false, false, false, unicode);
                else
                    InitFoundList(varopt, size / 8, false, false, false, unicode);
            else
                InitFoundList(varopt, size, false, false, false, unicode);
            if (scanopt != TScanOption.UnknownValue)
            {
                i = Math.Min((int)CountAddressesFound(), 10000000);
                lvScanner.VirtualListSize = i;
            }
            MessageBox.Show(CountAddressesFound().ToString());
            timer1.Enabled = true;
        }
        else
        {
            base.WndProc(ref m);
        }
    }

    public Main()
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
            InitMemoryScanner(Handle.ToInt32());
            MessageBox.Show("Process opened");
            scanopt = TScanOption.ExactValue;
            varopt = TVariableType.Dword;
            startscan = "$0000000000000000";
            endscan = "$7fffffffffffffff";
            unicode = false;
            casesensitive = false;
            btnNewScan.Enabled = true;
            btnFirstScan.Enabled = true;
        }
    }

    void BtnNewScan_Click(object sender, EventArgs e)
    {
        NewScan();
        btnNextScan.Enabled = false;
        btnFirstScan.Enabled = true;
        lvScanner.VirtualListSize = 0;
    }

    void BtnFirstScan_Click(object sender, EventArgs e)
    {
        TFastScanMethod fastscanmethod;
        TScanRegionPreference writable = TScanRegionPreference.Include,
            executable = TScanRegionPreference.DontCare, copyOnWrite = TScanRegionPreference.Exclude;
        timer1.Enabled = false;
        btnFirstScan.Enabled = false;

        switch (cbWritable.CheckState)
        {
            case CheckState.Unchecked: writable = TScanRegionPreference.Exclude; break;
            case CheckState.Checked: writable = TScanRegionPreference.Include; break;
            case CheckState.Indeterminate: writable = TScanRegionPreference.DontCare; break;
        }

        switch (cbExecutable.CheckState)
        {
            case CheckState.Unchecked: executable = TScanRegionPreference.Exclude; break;
            case CheckState.Checked: executable = TScanRegionPreference.Include; break;
            case CheckState.Indeterminate: executable = TScanRegionPreference.DontCare; break;
        }

        switch (cbCopyOnWrite.CheckState)
        {
            case CheckState.Unchecked: copyOnWrite = TScanRegionPreference.Exclude; break;
            case CheckState.Checked: copyOnWrite = TScanRegionPreference.Include; break;
            case CheckState.Indeterminate: copyOnWrite = TScanRegionPreference.DontCare; break;
        }

        ConfigScanner(writable, executable, copyOnWrite);

        if (cbFastScan.Checked)
        {
            if (rbAlignment.Checked)
                fastscanmethod = TFastScanMethod.Aligned;
            else
                fastscanmethod = TFastScanMethod.LastDigits;
        }
        else fastscanmethod = TFastScanMethod.NotAligned;

        FirstScan(scanopt, varopt, TRoundingType.Rounded, tbValue1.Text,
        tbValue2.Text, startscan, endscan, false, false, unicode, casesensitive,
        fastscanmethod, tbAlignment.Text);
    }

    void Timer1_Tick(object sender, EventArgs e)
    {
        ResetValues();
        lvScanner.Refresh();
    }

    void TbStartScan_TextChanged(object sender, EventArgs e)
    {
        startscan = '$' + tbStartScan.Text;
    }

    void TbEndScan_TextChanged(object sender, EventArgs e)
    {
        endscan = '$' + tbEndScan.Text;
    }

    void CbScanType_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (cbScanType.SelectedIndex)
        {
            case 0: scanopt = TScanOption.UnknownValue; break;
            case 1: scanopt = TScanOption.ExactValue; break;
            case 2: scanopt = TScanOption.ValueBetween; break;
            case 3: scanopt = TScanOption.BiggerThan; break;
            case 4: scanopt = TScanOption.SmallerThan; break;
            case 5: scanopt = TScanOption.IncreasedValue; break;
            case 6: scanopt = TScanOption.IncreasedValueBy; break;
            case 7: scanopt = TScanOption.DecreasedValue; break;
            case 8: scanopt = TScanOption.DecreasedValueBy; break;
            case 9: scanopt = TScanOption.Changed; break;
            case 10: scanopt = TScanOption.Unchanged; break;
        }
    }

    void CbValueType_SelectedIndexChanged(object sender, EventArgs e)
    {
        switch (cbValueType.SelectedIndex)
        {
            case 0: varopt = TVariableType.Binary; break;
            case 1: varopt = TVariableType.Byte; break;
            case 2: varopt = TVariableType.Word; break;
            case 3: varopt = TVariableType.Dword; break;
            case 4: varopt = TVariableType.Qword; break;
            case 5: varopt = TVariableType.Single; break;
            case 6: varopt = TVariableType.Double; break;
            case 7: varopt = TVariableType.String; break;
        }

        switch (varopt)
        {
            case TVariableType.Binary:
            case TVariableType.Byte:
            case TVariableType.String:
            case TVariableType.UnicodeString:
            case TVariableType.ByteArrays: tbAlignment.Text = "1"; break;
            case TVariableType.Word: tbAlignment.Text = "2"; break;
            default: tbAlignment.Text = "4"; break;
        }
    }

    void CbFastScan_CheckedChanged(object sender, EventArgs e)
    {
        tbAlignment.Enabled = cbFastScan.Checked && cbFastScan.Enabled;
        rbAlignment.Enabled = tbAlignment.Enabled;
        rbLastDigits.Enabled = tbAlignment.Enabled;
    }

    void LvScanner_RetrieveVirtualItem(object sender, RetrieveVirtualItemEventArgs e)
    {
        string address, value;
        try
        {
            ListViewItem lvi = new ListViewItem(); 	// create a listviewitem object
            GetAddress(e.ItemIndex, out address, out value);
            lvi.Text = address; 		// assign the text to the item
            ListViewItem.ListViewSubItem lvsi = new ListViewItem.ListViewSubItem(); // subitem
            lvsi.Text = value; 	// the subitem text
            lvi.SubItems.Add(lvsi); 			// assign subitem to item
            e.Item = lvi; 		// assign item to event argument's item-property
        }
        catch (Exception ex)
        {
        }

    }

    void FmScanner_Load(object sender, EventArgs e)
    {
        cbScanType.SelectedIndex = cbScanType.Items.IndexOf("Exact Value");
        cbValueType.SelectedIndex = cbValueType.Items.IndexOf("4 Bytes");

    }

    void BtnNextScan_Click(object sender, EventArgs e)
    {
        timer1.Enabled = false;
        btnNextScan.Enabled = false;
        NextScan(scanopt, TRoundingType.Rounded, tbValue1.Text, tbValue2.Text, false, false, unicode, casesensitive, false, false, "");
    }

    void CbUnicode_CheckedChanged(object sender, EventArgs e)
    {
        unicode = cbUnicode.Checked;
    }

    void CbCase_CheckedChanged(object sender, EventArgs e)
    {
        casesensitive = cbCase.Checked;
    }
}
