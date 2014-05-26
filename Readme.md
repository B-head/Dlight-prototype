#Dlight-prototype
Dlightは、初心者でも簡単に扱えるD言語を目指して構想していたものの、
いつの間にかD言語とは大きく違う文法になっていたプログラミング言語です。

[プロジェクトのWikiページ](https://github.com/B-head/Dlight-prototype/wiki)にも情報がありますので、合わせてお読み下さい。

##現在の目標
- 主要な静的型付け言語の基本的な機能、関数・クラス・ジェネリクスなどを実装する。
- 主要な動的型付け言語と同量程度のコードで、同等の動作を記述できるようにする。
- 今後の開発のために、開発・保守のしやすいコード設計を確立する。

##コード例
```
Random =: var rand //クラスインスタンスの生成と変数定義。
echo rand.value //メンバ変数を参照し出力。
echo rand.gen //メンバ関数を3回呼び出し出力。
echo rand.gen
echo rand.gen

class Random //クラス定義
{
	var value := 98765 //メンバ変数定義とデフォルト値の設定

	routine gen //関数定義
	{
		return value := value * 1103515245 + 12345
	}
}
```
