﻿using System;
using System.Xml;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections.Generic;

namespace Light.Data.Demo
{
	class MainClass
	{
		public static void Main (string[] args)
		{
			DataContext context = DataContext.Default;
			CommandOutput output = new CommandOutput ();
			output.OutputFullCommand = true;
			output.UseConsoleOutput = true;
			context.SetCommanfOutput (output);

			List<VehicleModel> list = context.LQuery<BusVehicleInfo> ().LeftJoin<BusFleetInfo> ()
				.On (BusVehicleInfo.FleetCodeField == BusFleetInfo.FleetCodeField)
				.SelectAll<BusVehicleInfo> ()
				.SelectAll<BusFleetInfo> ()
				.ToList<VehicleModel> ();


//			List<TestUser> list = context.LQuery<TestUser> ().Where (TestUser.IdField > 10 & TestUser.UserNameField == "aaa" & TestUser.RegTimeField >= DateTime.Now.Date).ToList ();
//			List<TaskModel> list = context.LQuery<TaskBase> ().Where (TaskBase.PriorityField == 100).LeftJoin<TaskContent> ()
//				.On (TaskBase.TaskIdField == TaskContent.TaskIdField)
//				.Select (TaskBase.TaskIdField, TaskBase.PositionField, TaskBase.PriorityField, TaskBase.StartTimeField, TaskBase.EndTimeField, TaskContent.ContentIdField, TaskContent.ContentTypeField, TaskContent.TransField, TaskContent.ContentField)
//				.Where (TaskContent.ContentTypeField != 1)
//				.OrderBy (TaskContent.ContentField.OrderByAsc ())
//				.ToList<TaskModel> ();

//			context.SelectInto<ScDevicePackageHistory,ScDevicePackage> (
//				new DataFieldInfo[] {
//					ScDevicePackageHistory.IdField,
//					ScDevicePackageHistory.PackageNameField,
//					ScDevicePackageHistory.RecordTimeField,
//					ScDevicePackageHistory.GetTimeField,
//					ScDevicePackageHistory.DataField,
//					ScDevicePackageHistory.CountField,
//					ScDevicePackageHistory.StatusField
//				},
//				new DataFieldInfo[] {
//					ScDevicePackage.IdField,
//					ScDevicePackage.PackageNameField,
//					ScDevicePackage.RecordTimeField,
//					DataFieldInfo.DbTimeDataField,
//					ScDevicePackage.DataField,
//					ScDevicePackage.CountField,
//					ScDevicePackage.StatusField
//				},
//				ScDevicePackage.IdField > 100 & ScDevicePackage.RecordTimeField >= DateTime.Now
//				, ScDevicePackage.IdField.OrderByAsc ());
			Console.ReadLine ();
		}

		public static string ToDBC (string input)
		{
			char[] c = input.ToCharArray ();
			for (int i = 0; i < c.Length; i++) {
				if (c [i] == 12288) {
					c [i] = (char)32;
					continue;
				}
				if (c [i] > 65280 && c [i] < 65375)
					c [i] = (char)(c [i] - 65248);
			}
			return new String (c);
		}

		static void ReadXml ()
		{
			XmlDocument doc = new XmlDocument ();
			StringBuilder sb = new StringBuilder ();
			doc.Load ("Re.xml");
			XmlNode root = doc.SelectNodes ("root") [0];
			foreach (XmlNode node in root.ChildNodes) {
				string name = node.Attributes ["name"].Value;
				string value = node.ChildNodes [1].InnerText;
				sb.AppendFormat ("/// <summary>\n\t\t/// {0}\n\t\t/// </summary>\n\t\t", value);
				string newValue = Regex.Replace (name, "[A-Z][a-z]", x => {
					return " " + x.Value.ToLower ();
				}, RegexOptions.Compiled);
				sb.AppendFormat ("public const string {0} = \"{1}\";\n\t\t", name, newValue);
			}
			string result = sb.ToString ();
			Console.WriteLine (result);
			Console.ReadLine ();
		}
	}
}
