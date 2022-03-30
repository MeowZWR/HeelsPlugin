﻿using Dalamud.Interface.Components;
using Dalamud.Logging;
using HeelsPlugin.Gui;
using ImGuiNET;
using Lumina.Excel.GeneratedSheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace HeelsPlugin
{
  public class PluginUI
  {
    private bool visible = false;
    public bool Visible
    {
      get => visible;
      set => visible = value;
    }
    private readonly IReadOnlyDictionary<uint, Item> feet;
    private readonly Dictionary<int, ConfigLine> configs = new();
    private int index = -1;
    private static readonly Vector4 GreyVector = new(0.5f, 0.5f, 0.5f, 1);

    private const int MAX_WINDOW_WIDTH = 645;

    public PluginUI()
    {
      feet = GameData.Equipment(Plugin.Data);

      try
      {
        var configs = Plugin.Configuration.Configs;

        if (configs.Count > 0)
        {
          for (var i = 0; i < configs.Count; i++)
          {
            AddConfigLine(configs[i]);
          }
        }
        else
        {
          AddConfigLine();
        }
      }
      catch (Exception ex)
      {
        PluginLog.Error(ex, "Failed to create PluginUI");
      }
    }

    public void Draw()
    {
      try
      {
        DrawConfiguration();
      }
      catch
      {
      }
    }

    private ComboWithFilter<Item> CreateCombo(int id)
    {
      var fontScale = ImGui.GetIO().FontGlobalScale;
      return new($"##FeetPics{id}", -1, -1, feet.Values.ToArray(), i => i.Name)
      {
        Flags = ImGuiComboFlags.HeightLarge,
        CreateSelectable = i =>
        {
          var ret = ImGui.Selectable(i.Name);
          var model = new Quad(i.ModelMain);
          var ids = $"{model.A}, {model.B}";
          var size = ImGui.CalcTextSize(ids).X;
          ImGui.SameLine(ImGui.GetWindowContentRegionWidth() - size - ImGui.GetStyle().ItemInnerSpacing.X);
          ImGui.TextColored(GreyVector, ids);
          return ret;
        }
      };
    }

    private void AddConfigLine(ConfigModel? model = null)
    {
      index++;
      ConfigLine line;
      if (model == null)
        line = new(index, CreateCombo(index));
      else
        line = new(index, CreateCombo(index), model);
      line.OnDelete += HandleDelete;
      line.OnChange += HandleChange;
      configs.Add(index, line);
    }

    private void HandleDelete(int key)
    {
      configs.Remove(key);
    }

    private void HandleChange()
    {
      Plugin.Configuration.Configs = configs.Values.Select(c => c.Model).ToList();
      Plugin.Configuration.Save();
    }

    public void DrawConfiguration()
    {
      if (!visible) return;

      var fontScale = ImGui.GetIO().FontGlobalScale;
      var size = new Vector2(MAX_WINDOW_WIDTH * fontScale, 200 * fontScale);

      ImGui.SetNextWindowSize(size, ImGuiCond.FirstUseEver);
      ImGui.SetNextWindowSizeConstraints(size, new Vector2((MAX_WINDOW_WIDTH + 600) * fontScale, 1000 * fontScale));

      if (ImGui.Begin("HeelsPlugin Config", ref visible))
      {
        if (ImGui.Button("Add Entry", new Vector2(-1, 24 * fontScale)))
          AddConfigLine();

        ImGui.Spacing();
        ImGui.Separator();
        ImGui.Spacing();

        ImGui.PushStyleVar(ImGuiStyleVar.CellPadding, new Vector2(5f, 6f));
        ImGui.BeginChild("##ConfigScroll");
        if (ImGui.BeginTable("##Config", 5, ImGuiTableFlags.Borders | ImGuiTableFlags.Resizable))
        {
          ImGui.TableSetupColumn("Enabled", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
          ImGui.TableSetupColumn("Name");
          ImGui.TableSetupColumn("Item");
          ImGui.TableSetupColumn("Height", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
          ImGui.TableSetupColumn("Remove", ImGuiTableColumnFlags.WidthFixed | ImGuiTableColumnFlags.NoResize);
          ImGui.TableHeadersRow();

          foreach (var config in configs)
            config.Value.Draw();
        }
        ImGui.EndTable();
        ImGui.EndChild();
        ImGui.PopStyleVar();
      }
    }
  }
}
