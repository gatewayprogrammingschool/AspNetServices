using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media.Media3D;

using Microsoft.Xaml.Behaviors;
using Microsoft.Xaml.Behaviors.Core;

using Syncfusion.UI.Xaml.Grid;
using Syncfusion.UI.Xaml.TreeGrid;

namespace MarkdownTree;

public class SfTreeGridBehavior : ConditionBehavior
{
    private SfTreeGrid datagrid;

    SfTreeGridBehavior(SfTreeGrid treeGrid)
    {
        datagrid = treeGrid;
        treeGrid.RowValidating += TreeGrid_RowValidating; ;
    }

    private void TreeGrid_RowValidating(object sender, TreeGridRowValidatingEventArgs e)
    {
    }
}