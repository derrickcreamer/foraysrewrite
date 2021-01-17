using System;
using System.Text;
using System.Collections.Generic;

namespace GrammarUtility{
	public enum Determinative { None, The, AAn, ThisThese, ThatThose, Your };
	public class Grammar{
		private class NounInfo{
			public string Plural;
			public bool UsesAn, Uncountable, NoArticles;
		}

		private Dictionary<string, NounInfo> nouns;
		private Dictionary<string, string> verbs; // Base verb -> third person singular
		private StringBuilder sb;

		public Grammar(){
			nouns = new Dictionary<string, NounInfo>();
			verbs = new Dictionary<string, string>();
			sb = new StringBuilder();
		}
		///<summary>Registers a noun for later use, using a template like "pair~ of boots". Return value is the final singular form, such as "pair of boots".</summary>
		/// <param name="template">"brick" is pluralized as "bricks". "juicy peach" is pluralized as "juicy peaches".
		/// "berry" is pluralized as "berries". Tilde characters can be used if the pluralization should happen elsewhere within the word:
		/// "pair~ of boots" becomes "pair of boots" and "pairs of boots". "batch~~ of dough" becomes "batch of dough" and "batches of dough".
		/// If the character immediately preceding the 2 tildes is a 'y', then the "ies" rule is used instead.</param>
		/// <param name="exceptionToAAnRule">Default is to check for [aeiouAEIOU]. This bool negates that rule.</param>
		/// <param name="uncountable">Marks uncountable nouns like "water", "courage", and "equipment". These names don't receive quantities or "a/an".</param>
		/// <param name="noArticles">Indistinct or unique names might not accept articles, like "something" or "Excalibur".</param>
		public string AddNoun(string template, bool exceptionToAAnRule = false, bool uncountable = false, bool noArticles = false){
			if(string.IsNullOrEmpty(template)) throw new ArgumentException("Template missing");
			string singular, plural;
			if(template.Contains("~")){
				singular = template.Replace("y~~", "y").Replace("~", "");
				plural = template.Replace("y~~", "ies").Replace("~~", "es").Replace("~", "s");
			}
			else{
				singular = template;
				plural = (uncountable || noArticles)? template : GetWithS(template);
			}
			AddNoun(singular, plural, exceptionToAAnRule, uncountable, noArticles);
			return singular;
		}
		///<summary>Registers a noun for later lookup and use, with the given singular and plural.</summary>
		/// <param name="singular">The base noun that will be used to look up the rest of the values passed in as arguments.</param>
		/// <param name="plural">The plural form of the base noun.</param>
		/// <param name="exceptionToAAnRule">Default is to check for [aeiouAEIOU]. This bool negates that rule.</param>
		/// <param name="uncountable">Marks uncountable nouns like "water", "courage", and "equipment". These names don't receive quantities or "a/an".</param>
		/// <param name="noArticles">Indistinct or unique names might not accept articles, like "something" or "Excalibur".</param>
		public void AddNoun(string singular, string plural, bool exceptionToAAnRule = false, bool uncountable = false, bool noArticles = false){
			if(string.IsNullOrEmpty(singular)) throw new ArgumentException("Singular missing");
			if(string.IsNullOrEmpty(plural)) throw new ArgumentException("Plural missing");
			bool usesAn = StartsWithVowel(singular);
			if(exceptionToAAnRule) usesAn = !usesAn;
			nouns.Add(singular, new NounInfo{ Plural = plural, UsesAn = usesAn, Uncountable = uncountable, NoArticles = noArticles });
		}
		///<summary>Registers a verb for later lookup and use, using a template like "sign~ up". Return value is the final singular form, such as "sign up".</summary>
		/// <param name="template">"attack" is automatically assigned a third person singular form of "attacks". Likewise "catch" and "catches",
		/// and "parry" and "parries". Tilde characters can be used to apply these rules elsewhere within the word: "sign~ up" or "vary~~ in quality".</param>
		public string AddVerb(string template){
			if(string.IsNullOrEmpty(template)) throw new ArgumentException("Template missing");
			string verb, thirdPersonSingular;
			if(template.Contains("~")){
				verb = template.Replace("y~~", "y").Replace("~", "");
				thirdPersonSingular = template.Replace("y~~", "ies").Replace("~~", "es").Replace("~", "s");
			}
			else{
				verb = template;
				thirdPersonSingular = GetWithS(template);
			}
			verbs.Add(verb, thirdPersonSingular);
			return verb;
		}
		///<summary>Registers a verb for later lookup and use.</summary>
		/// <param name="baseVerb">For example, "attack".</param>
		/// <param name="thirdPersonSingular">For example, "attacks".</param>
		public void AddVerb(string baseVerb, string thirdPersonSingular){
			if(string.IsNullOrEmpty(baseVerb)) throw new ArgumentException("Base verb missing");
			if(string.IsNullOrEmpty(thirdPersonSingular)) throw new ArgumentException("Third person singular verb missing");
			verbs.Add(baseVerb, thirdPersonSingular);
		}
		///<summary>Construct and return a sentence fragment from the given info.</summary>
		/// <param name="extraText">If present, will be added to the result string before the verb, but after everything else.
		/// For example, if extraText is "(unaware)", the result string could be "the goblin (unaware) trips". (This allows additional text
		/// to be included while still using the value of 'noun' to perform the lookup.)</param>
		/// <param name="excludeQuantity">If true, the value of 'quantity' will only be used to determine singular vs. plural, but
		/// will not be included in the result string.</param>
		public string Get(Determinative determinative, string noun, string verb = null, string extraText = null, int quantity = 1,
			bool excludeQuantity = false, bool possessive = false)
		{
			NounInfo nounInfo;
			nouns.TryGetValue(noun, out nounInfo);
			bool noArticles = nounInfo != null && nounInfo.NoArticles; // Default false
			bool uncountable = nounInfo != null && nounInfo.Uncountable; // Default false
			if(!noArticles){
				if(determinative == Determinative.The){
					sb.Append("the ");
					if(!excludeQuantity && quantity != 1){
						sb.Append(quantity);
						sb.Append(" ");
					}
				}
				else if(determinative == Determinative.AAn){
					if(uncountable) sb.Append("some ");
					else{
						if(quantity != 1){
							if(!excludeQuantity){
								sb.Append(quantity);
								sb.Append(" ");
							}
						}
						else{
							bool usesAn;
							if(nounInfo != null) usesAn = nounInfo.UsesAn;
							else usesAn = StartsWithVowel(noun);

							if(usesAn) sb.Append("an ");
							else sb.Append("a ");
						}
					}
				}
				else if(determinative == Determinative.ThisThese){
					if(quantity != 1){
						sb.Append("these ");
						if(!excludeQuantity){
							sb.Append(quantity);
							sb.Append(" ");
						}
					}
					else sb.Append("this ");
				}
				else if(determinative == Determinative.ThatThose){
					if(quantity != 1){
						sb.Append("those ");
						if(!excludeQuantity){
							sb.Append(quantity);
							sb.Append(" ");
						}
					}
					else sb.Append("that ");
				}
				else if(determinative == Determinative.Your){
					sb.Append("your ");
					if(!excludeQuantity){
						sb.Append(quantity);
						sb.Append(" ");
					}
				}
			}
			if(quantity == 1 || uncountable || noArticles){
				sb.Append(noun);
			}
			else{
				if(nounInfo != null){
					sb.Append(nounInfo.Plural);
				}
				else{
					AppendWithS(noun);
				}
			}
			if(possessive){
				if(noun == "you" || noun == "You") sb.Append("r"); // "your"
				else sb.Append("'s");
			}
			if(extraText != null){
				sb.Append(" "); // Add this space only if needed, to avoid trailing spaces on the returned value.
				sb.Append(extraText);
			}
			if(verb != null){
				sb.Append(" ");
				if(quantity != 1 || noun == "you" || noun == "You"){
					sb.Append(verb);
				}
				else{
					string thirdPersonSingular;
					if(verbs.TryGetValue(verb, out thirdPersonSingular)) sb.Append(thirdPersonSingular);
					else AppendWithS(verb);
				}
			}
			string result = sb.ToString();
			sb.Clear();
			return result;
		}
		private void AppendWithS(string word){
			if(word.EndsWith("sh") || word.EndsWith("ch") || word.EndsWith("s") || word.EndsWith("z") || word.EndsWith("x")){
				sb.Append(word);
				sb.Append("es");
			}
			else if(word.EndsWith("y") && !word.EndsWith("ay") && !word.EndsWith("ey") && !word.EndsWith("oy") && !word.EndsWith("uy")){
				sb.Append(word.Substring(0, word.Length - 1));
				sb.Append("ies");
			}
			else{
				sb.Append(word);
				sb.Append("s");
			}
		}
		private static string GetWithS(string word){
			if(word.EndsWith("sh") || word.EndsWith("ch") || word.EndsWith("s") || word.EndsWith("z") || word.EndsWith("x")){
				return word + "es";
			}
			else if(word.EndsWith("y") && !word.EndsWith("ay") && !word.EndsWith("ey") && !word.EndsWith("oy") && !word.EndsWith("uy")){
				return word.Substring(0, word.Length - 1) + "ies";
			}
			else{
				return word + "s";
			}
		}
		private static bool StartsWithVowel(string word){
			char first = word[0];
			return (first == 'a' || first == 'e' || first == 'i' || first == 'o' || first == 'u'
				|| first == 'A' || first == 'E' || first == 'I' || first == 'O' || first == 'U');
		}
	}
}
