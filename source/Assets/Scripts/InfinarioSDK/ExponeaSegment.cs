using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace Infinario
{
	class ExponeaSegment
	{
        private string _name;
	    private string _analysis_name;
	    private int _index;

        public ExponeaSegment(string name, string analysisName, int index){
            this._name = name;
            this._analysis_name = analysisName;
            this._index = index;
        }

        public string GetName(){
            return this._name;
        }

        public string GetSegmentationName()
        {
            return this._analysis_name;
        }

        public int? GetSegmentIndex()
        {
            return this._index;
        }

	    public override string ToString()
	    {
	        return "Segment: " + _name + ", Segment Index: " + _index + ", Segmentation name: " + _analysis_name;
	    }
	}
}
