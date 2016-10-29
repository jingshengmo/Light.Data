﻿using System;
using NUnit.Framework;
using System.Collections.Generic;
using System.Collections;
using Light.Data.UnitTest;

namespace Light.Data.MysqlTest
{
	[TestFixture ()]
	public class TT_BaseCommandTest : BaseTest
	{
		[Test ()]
		public void TestCase_SaveErase_Single ()
		{
			context.TruncateTable<TeUser> ();

			TeUser userInsert = CreateTestUser (true);
			userInsert.Save ();
			Assert.Greater (userInsert.Id, 0);
			TeUser user1 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.NotNull (user1);
			AssertExtend.AreTypeEqual (userInsert, user1);
			user1.LastLoginTime = GetNow ();
			user1.Status = 2;
			user1.Save ();
			Assert.AreEqual (userInsert.Id, user1.Id);
			TeUser user2 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.NotNull (user2);
			AssertExtend.AreTypeEqual (user1, user2);
			user2.Erase ();
			TeUser user3 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.Null (user3);
		}

		[Test ()]
		public void TestCase_CUD_Single ()
		{
			context.TruncateTable<TeUser> ();

			TeUser userInsert = CreateTestUser (true);
			context.Insert (userInsert);
			Assert.Greater (userInsert.Id, 0);
			TeUser user1 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.NotNull (user1);
			AssertExtend.AreTypeEqual (userInsert, user1);
			user1.LastLoginTime = GetNow ();
			user1.Status = 2;
			context.Update (user1);
			Assert.AreEqual (userInsert.Id, user1.Id);
			TeUser user2 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.NotNull (user2);
			AssertExtend.AreTypeEqual (user1, user2);
			context.Delete (user2);
			TeUser user3 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			Assert.Null (user3);
		}

		[Test ()]
		public void TestCase_CUD_Single_NoIdentity ()
		{
			context.TruncateTable<TeUserLevel> ();

			TeUserLevel levelInsert = context.CreateNew<TeUserLevel> ();
			levelInsert.Id = 1;
			levelInsert.LevelName = "level1";
			levelInsert.Status = 1;
			context.Insert (levelInsert);
			Assert.AreEqual (1, levelInsert.Id);
			TeUserLevel level1 = context.SelectSingleFromKey<TeUserLevel> (levelInsert.Id);
			Assert.NotNull (level1);
			AssertExtend.AreObjectsEqual (levelInsert, level1);
			level1.Status = 2;
			context.Update (level1);
			Assert.AreEqual (1, levelInsert.Id);
			TeUserLevel level2 = context.SelectSingleFromKey<TeUserLevel> (levelInsert.Id);
			Assert.NotNull (level2);
			AssertExtend.AreObjectsEqual (level1, level2);
			context.Delete (level2);
			TeUserLevel level3 = context.SelectSingleFromKey<TeUserLevel> (levelInsert.Id);
			Assert.Null (level3);
		}


		[Test ()]
		public void TestCase_InsertOrUpdate_Single ()
		{
			context.TruncateTable<TeUser> ();

			TeUser userInsert = CreateTestUser (true);
			context.Insert (userInsert);
			Assert.Greater (userInsert.Id, 0);
			int id = userInsert.Id;
			userInsert.Account = "abc";
			context.InsertOrUpdate (userInsert);
			Assert.AreEqual (id, userInsert.Id);
			TeUser user1 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			AssertExtend.AreTypeEqual (userInsert, user1);

			userInsert.Id = 0;
			context.InsertOrUpdate (userInsert);
			Assert.AreEqual (id + 1, userInsert.Id);
			TeUser user2 = context.SelectSingleFromId<TeUser> (userInsert.Id);
			AssertExtend.AreTypeEqual (userInsert, user2);

		}

		[Test ()]
		public void TestCase_BulkInsert ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (count, result);
			listAc = context.Query<TeUser> ().ToList ();

			AssertExtend.AreEnumerableEqual (listEx, listAc);

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray (), 20);
			Assert.AreEqual (result, count);
			listAc = context.Query<TeUser> ().ToList ();
			AssertExtend.AreEnumerableEqual (listEx, listAc);

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray (), 100);
			Assert.AreEqual (result, count);
			listAc = context.Query<TeUser> ().ToList ();
			AssertExtend.AreEnumerableEqual (listEx, listAc);
		}

		[Test ()]
		public void TestCase_UpdateMass ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (result, count);

			DateTime uptime = GetNow ();

			result = context.Query<TeUser> ()
							.Update (x => new TeUser {
								LastLoginTime = uptime,
								Status = 2
							});
			Assert.AreEqual (count, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count, listAc.Count);
			Assert.IsTrue (listAc.TrueForAll (x => x.LastLoginTime == uptime && x.Status == 2));

			const int rdd = 20;

			result = context.Query<TeUser> ().Where (x => x.Id >= listEx [0].Id && x.Id <= listEx [0].Id + rdd - 1)
							.Update (x => new TeUser {
								Status = 3
							});

			Assert.AreEqual (rdd, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count, listAc.Count);
			Assert.IsTrue (listAc.TrueForAll (x => {
				if (x.Id <= rdd) {
					return x.Status == 3;
				}
				else {
					return true;
				}
			}));

			result = context.Query<TeUser> ().Update (x => new TeUser {
				Area = 4,
				CheckLevelType = CheckLevelType.High
			});
			Assert.AreEqual (count, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count, listAc.Count);
			Assert.IsTrue (listAc.TrueForAll (x => x.Area == 4 && x.CheckLevelType == CheckLevelType.High));

			result = context.Query<TeUser> ().Where (x => x.Id >= listEx [0].Id && x.Id <= listEx [0].Id + rdd - 1).Update (x => new TeUser {
				Status = 6
			});
			Assert.AreEqual (rdd, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count, listAc.Count);
			Assert.IsTrue (listAc.TrueForAll (x => {
				if (x.Id <= rdd) {
					return x.Status == 6;
				}
				else {
					return true;
				}
			}));
		}


		[Test ()]
		public void TestCase_DeleteMass1 ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (result, count);

			result = context.Query<TeUser> ().Delete ();
			Assert.AreEqual (count, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (0, listAc.Count);
		}


		[Test ()]
		public void TestCase_DeleteMass2 ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (result, count);

			result = context.Query<TeUser> ().Delete ();
			Assert.AreEqual (count, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (0, listAc.Count);
		}

		[Test ()]
		public void TestCase_DeleteMass3 ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;
			const int rdd = 20;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (result, count);

			result = context.Query<TeUser> ().Where (x => x.Id >= listEx [0].Id && x.Id <= listEx [0].Id + rdd - 1).Delete ();
			Assert.AreEqual (rdd, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count - rdd, listAc.Count);
		}

		[Test ()]
		public void TestCase_DeleteMass4 ()
		{
			List<TeUser> listEx = new List<TeUser> ();
			const int count = 33;
			for (int i = 0; i < count; i++) {
				TeUser userInsert = CreateTestUser (false);
				userInsert.Account += i;
				userInsert.RegTime = userInsert.RegTime.AddSeconds (i);
				listEx.Add (userInsert);
			}

			int result;
			List<TeUser> listAc;
			const int rdd = 20;

			context.TruncateTable<TeUser> ();
			result = context.BatchInsert (listEx.ToArray ());
			Assert.AreEqual (result, count);

			result = context.Query<TeUser> ().Where (x => x.Id >= listEx [0].Id && x.Id <= listEx [0].Id + rdd - 1).Delete ();
			Assert.AreEqual (rdd, result);
			listAc = context.Query<TeUser> ().ToList ();
			Assert.AreEqual (count - rdd, listAc.Count);
		}


	}
}
