using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PossibleTask {

    public string sRawDescription;
    public int nMinValue;
    public int nMaxValue;
    public int nMinDifficulty;
    public int nMaxDifficulty;
    public int nMaxCount;
    public int nMinDelta;
    public float fFrequencyModifier;
    public string sURL;

    public PossibleTask() {
        //We'll initialize everything to a default value - the log file will
        //  then overwrite these as we continue through parsing the line

        sRawDescription = "";
        nMinValue = 1;
        nMaxValue = 1;
        nMinDifficulty = -1;
        nMaxDifficulty = -1;
        nMaxCount = 1;
        nMinDelta = 1;
        fFrequencyModifier = 1f;
        sURL = "";
    }

    public void SetRawDescription(string _sRawDescription) {
        sRawDescription = _sRawDescription;
    }

    public void SetMinValue(int _nMinValue) {
        nMinValue = _nMinValue;
    }
    public void SetMaxValue(int _nMaxValue) {
        nMaxValue = _nMaxValue;
    }

    public void SetMinDifficulty(int _nMinDifficulty) {
        nMinDifficulty = _nMinDifficulty;
    }
    public void SetMaxDifficulty(int _nMaxDifficulty) {
        nMaxDifficulty = _nMaxDifficulty;
    }
    public void SetFixedDifficulty(int _nDifficulty) {
        nMinDifficulty = _nDifficulty;
        nMaxDifficulty = _nDifficulty;
    }

    public void SetMaxCount(int _nMaxCount) {
        nMaxCount = _nMaxCount;
    }
    public void SetMinDelta(int _nMinDelta) {
        nMinDelta = _nMinDelta;
    }

    public void SetFrequencyModifier(float _fFrequencyModifier) {
        fFrequencyModifier = _fFrequencyModifier;
    }

    public void SetURL(string _sURL) {
        sURL = _sURL;
    }

    public bool IsSufficientlyFilledOut() {

        if(sRawDescription == "") {
            Debug.LogErrorFormat("Missing Description!");
            return false;
        }

        if(nMinDifficulty == -1) {
            Debug.LogErrorFormat("Missing Minimum Difficulty for {0}", sRawDescription);
            return false;
        }

        if(nMaxDifficulty == -1) {
            Debug.LogErrorFormat("Missing Maximum Difficulty for {0}", sRawDescription);
            return false;
        }

        return true;
    }

    public override string ToString() {
        if(nMinValue != nMaxValue) {
            return string.Format("<Desc:{0},Value:{1}-{2},Diff:{3}-{4},Max-count:{5},Min-delta:{6},Freq:{7}>",
                sRawDescription, nMinValue, nMaxValue, nMinDifficulty, nMaxDifficulty, nMaxCount, nMinDelta, fFrequencyModifier);
        } else {
            return string.Format("<Desc:{0},Diff:{1},Max-count:{2},Min-delta:{3},Freq:{4}>",
                sRawDescription, nMinDifficulty, nMaxCount, nMinDelta, fFrequencyModifier);
        }
    }


}
