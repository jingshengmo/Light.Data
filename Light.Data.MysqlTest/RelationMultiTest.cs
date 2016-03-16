﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using Light.Data;

namespace Light.Data.MysqlTest
{
	[TestFixture ()]
	public class RelationMultiTest:BaseTest
	{
		[Test ()]
		public void TestCase_CollectionAndSingle ()
		{
			InitialUserTable (40);
			InitialUserLevelTable (10);


			List<TeUser> users;
			List<TeUserLevel> levels;
			Dictionary<int,List<TeUser>> dict;
			List<TeUserLevelWithUserRefer> list; 


			users = context.LQuery<TeUser> ().ToList ();
			levels = context.LQuery<TeUserLevel> ().ToList ();
			dict = new Dictionary<int, List<TeUser>> ();
			foreach (TeUserLevel level in levels) {
				dict [level.Id] = users.FindAll (x => x.LevelId == level.Id);
			}
			list = context.LQuery<TeUserLevelWithUserRefer> ().ToList ();
			Assert.AreEqual (dict.Count, list.Count);
			foreach (KeyValuePair<int,List<TeUser>> kvs in dict) {
				TeUserLevelWithUserRefer lu = list.Find (x => x.Id == kvs.Key);
				Assert.NotNull (lu);
				List<TeUserWithLevelRefer> us = new List<TeUserWithLevelRefer> ();
				us.AddRange (lu.Users);
				Assert.AreEqual (kvs.Value.Count, us.Count);
				for (int i = 0; i < us.Count; i++) {
					Assert.IsTrue (EqualUser (kvs.Value [i], us [i]));
					Assert.NotNull (us [i].UserLevel);
					Assert.AreEqual (lu, us [i].UserLevel);
				}

			}
		}


		[Test ()]
		public void TestCase_SingleAndSingle ()
		{
			InitialUserTable (40);
			InitialUserExtendTable (30);


			List<TeUser> users;
			List<TeUserExtend> extends;
			List<TeUserWithExtendRefer> list; 


			users = context.LQuery<TeUser> ().ToList ();
			extends = context.LQuery<TeUserExtend> ().ToList ();

			list = context.LQuery<TeUserWithExtendRefer> ().ToList ();
			Assert.AreEqual (users.Count, list.Count);
			foreach (TeUser user in users) {
				TeUserExtend extend = extends.Find (x => x.UserId == user.Id);
				TeUserWithExtendRefer refer = list.Find (x => x.Id == user.Id);
				Assert.IsTrue (EqualUser (user, refer));
				if (extend == null) {
					Assert.IsNull (refer.UserExtend);
				}
				else {
					Assert.IsTrue (EqualUserExtend(extend, refer.UserExtend));
					Assert.AreEqual (refer.UserExtend.User, refer);
				}
			}
		}
	}
}