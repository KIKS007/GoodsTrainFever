using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
public class GetLDValues : MonoBehaviour {

[Button]
void GetValue(){
	Transform parent = this.transform;

	string things;
	string at1 = "";
	string at2 = "";
	string at3 = "";
	string at4 = "";
	string at5 = "";
	foreach(Transform t in parent){
		if(t.gameObject.GetComponent<LevelHandmade>()){
		LevelHandmade lhm =  t.gameObject.GetComponent<LevelHandmade>();
		int t1 =0;

		foreach(Train_Level tl in lhm.rail1Trains){
			t1 += tl.trainDuration;
		}
		foreach(Train_Level tl in lhm.rail2Trains){
			t1 += tl.trainDuration;
		}
		var t2 = lhm.orders.Count;
		var t3 = lhm.rail1Trains.Count + lhm.rail2Trains.Count;
		var t4 = lhm.boats.Count;
		var t5 = lhm.errorsAllowed;
		at1 += t1 +"	";
		at2 += t2 +"	";
		at3 += t3 +"	";
		at4 += t4 +"	";
		at5 += t5 +"	";
		}


		if(t.gameObject.GetComponent<LevelSettings_LD>()){
		LevelSettings_LD lsld =  t.gameObject.GetComponent<LevelSettings_LD>();
		var t1 = lsld.levelDuration;
		var t2 = "["+lsld.ordersCountMin+","+lsld.ordersCountMax+"]";
		var t3 = "["+lsld.trainsCountMin+","+lsld.trainsCountMax+"]";
		var t4 = "NO";
		var t5 = lsld.errorsAllowed;
		at1 += t1 +"	";
		at2 += t2 +"	";
		at3 += t3 +"	";
		at4 += t4 +"	";
		at5 += t5 +"	";
		}
	}
	things = at1 + "\n" + at2 + "\n" + at3 + "\n" + at4 + "\n" + at5;
	
	System.IO.File.WriteAllText ("Assets/" + "LDValue" + ".txt", things);
}



}
