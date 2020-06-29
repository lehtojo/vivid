
class TestConfigurationPhase : Phase
{
   public override Status Execute(Bundle bundle)
   {
      var contents = new string[] { bundle.Get("code", string.Empty) };
      bundle.Put("input_file_contents", contents);

      return Status.OK;
   }
}