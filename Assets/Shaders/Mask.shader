Shader "Custom/Mask"
{
    SubShader
    {
        Tags {"Queue"="Transparent+1"}
        Pass{
            blend Zero One
        }
    }
    FallBack "Diffuse"
}